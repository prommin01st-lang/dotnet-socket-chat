using ChatBackend.Data;
using ChatBackend.Entities;
using ChatBackend.Models;
using ChatBackend.Services; // (!!!) 1. (เพิ่ม) Import Service
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class UsersController : ControllerBase
    {
        // (!!!) 2. (เปลี่ยน)
        // (ลบ UserManager ออก)
        private readonly IUserService _userService;

        // (!!!) 3. (เปลี่ยน)
        // (ฉีด (Inject) IUserService แทน)
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return Unauthorized();
            }
            
            // (!!!) 4. (เปลี่ยน)
            // (เรียก Service (สมอง) แทนการ Query เอง)
            var users = await _userService.GetAllUsersAsync(currentUserId);
            
            return Ok(users);
        }
    }
}