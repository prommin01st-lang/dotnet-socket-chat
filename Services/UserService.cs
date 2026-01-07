using ChatBackend.Data;
using ChatBackend.Entities;
using ChatBackend.Helpers;
using ChatBackend.Models;
using ChatBackend.Services.FileStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBackend.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileStorageService _fileStorageService;

        public UserService(UserManager<ApplicationUser> userManager, IFileStorageService fileStorageService)
        {
            _userManager = userManager;
            _fileStorageService = fileStorageService;
        }

        // 1. Get All Users (Pagination + Search)
        public async Task<PagedList<ParticipantDto>> GetUsersAsync(PaginationParams paginationParams, string? currentUserId)
        {
            var query = _userManager.Users.AsQueryable();

            // Filter out Soft Deleted users
            query = query.Where(u => !u.IsDeleted);

            // Search Logic
            if (!string.IsNullOrEmpty(paginationParams.SearchTerm))
            {
                var term = paginationParams.SearchTerm.ToLower();
                query = query.Where(u => 
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(term)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(term)));
            }

            // Exclude current user (optional, depend on requirement)
            if (!string.IsNullOrEmpty(currentUserId))
            {
                query = query.Where(u => u.Id != currentUserId);
            }

            // Project to DTO
            var projectedQuery = query.Select(u => new ParticipantDto
            {
                Id = u.Id,
                Email = u.Email!,
                FirstName = u.FirstName ?? "Unknown",
                LastName = u.LastName ?? "User",
                ProfilePictureUrl = u.ProfilePictureUrl
            });

            // Pagination using Helper
            return await PagedList<ParticipantDto>.CreateAsync(
                projectedQuery, 
                paginationParams.PageNumber, 
                paginationParams.PageSize);
        }

        // 2. Get Single User
        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
             // Allow getting deleted user? If no -> .Where(u => !u.IsDeleted)
             return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        }

        // 3. Update User
        public async Task<ApplicationUser?> UpdateUserAsync(string userId, UserUpdateDto userUpdateDto, string currentUserId, bool isAdmin)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return null;

            // Authorization Check: Only Admin or Owner can update
            if (!isAdmin && user.Id != currentUserId)
            {
                throw new UnauthorizedAccessException("You can only update your own profile.");
            }

            user.FirstName = userUpdateDto.FirstName ?? user.FirstName;
            user.LastName = userUpdateDto.LastName ?? user.LastName;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ? user : null;
        }

        // 4. Soft Delete
        public async Task<bool> SoftDeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsDeleted = true;
            user.IsActive = false; // Optional: Deactivate login as well

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        // 5. Hard Delete
        public async Task<bool> HardDeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        // 6. Upload Profile Picture
        public async Task<string> UploadProfilePictureAsync(string userId, IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            // 1. Generate unique filename
            var fileName = $"profile-pics/{userId}-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // 2. Upload to Cloudflare R2
            var fileUrl = await _fileStorageService.UploadFileAsync(file, fileName);

            // 3. Update User Profile
            user.ProfilePictureUrl = fileUrl;
            await _userManager.UpdateAsync(user);

            return fileUrl;
        }

        // 7. Role Management
        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> AssignRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Check if role exists (optional validation, but Identity handles it)
            // But we can just try adding
            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<bool> RemoveRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }

        // Backward Compatibility
        public async Task<IEnumerable<ParticipantDto>> GetAllUsersAsync(string currentUserId)
        {
             return (await GetUsersAsync(new PaginationParams { PageSize = 100 }, currentUserId));
        }
    }
}