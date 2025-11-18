using ChatBackend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBackend.Services
{
    // (1) (ใหม่) สร้าง Interface สำหรับ User Management
    public interface IUserService
    {
        Task<IEnumerable<ParticipantDto>> GetAllUsersAsync(string currentUserId);

        // (ในอนาคต, เราสามารถเพิ่ม Method สำหรับ "User Management" ที่นี่)
        // Task<UserDto> GetUserByIdAsync(string userId);
        // Task<bool> UpdateUserRolesAsync(string userId, List<string> newRoles);
        // Task<bool> DeactivateUserAsync(string userId);
    }
}