using ChatBackend.Data;
using ChatBackend.Entities;
using ChatBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBackend.Services
{
    // (2) (ใหม่) สร้าง Implementation
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<ParticipantDto>> GetAllUsersAsync(string currentUserId)
        {
            // (ย้าย Logic (เดิม) จาก Controller มาไว้ที่นี่)
            var users = await _userManager.Users
                .Where(u => u.Id != currentUserId) 
                .Select(u => new ParticipantDto 
                {
                    Id = u.Id,
                    Email = u.Email!,
                    FirstName = u.FirstName ?? "Unknown",
                    LastName = u.LastName ?? "User",
                    ProfilePictureUrl = u.ProfilePictureUrl
                })
                .ToListAsync();
            
            return users;
        }

        // (ในอนาคต, เราสามารถเพิ่ม Logic (CRUD) ที่นี่)
    }
}