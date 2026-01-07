using ChatBackend.Entities;
using ChatBackend.Helpers;
using ChatBackend.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBackend.Services
{
    public interface IUserService
    {
        Task<PagedList<ParticipantDto>> GetUsersAsync(PaginationParams paginationParams, string? currentUserId);
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> UpdateUserAsync(string userId, UserUpdateDto userUpdateDto, string currentUserId, bool isAdmin);
        Task<bool> SoftDeleteUserAsync(string userId);
        Task<bool> HardDeleteUserAsync(string userId);
        Task<string> UploadProfilePictureAsync(string userId, IFormFile file);
        
        // Role Management
        Task<IList<string>> GetUserRolesAsync(string userId);
        Task<bool> AssignRoleAsync(string userId, string roleName);
        Task<bool> RemoveRoleAsync(string userId, string roleName);

        // Backward compatibility
        Task<IEnumerable<ParticipantDto>> GetAllUsersAsync(string currentUserId);
    }
}