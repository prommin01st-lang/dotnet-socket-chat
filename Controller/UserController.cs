using ChatBackend.Helpers;
using ChatBackend.Models;
using ChatBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] PaginationParams paginationParams)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var users = await _userService.GetUsersAsync(paginationParams, currentUserId);
            
            Response.Headers.Append("X-Pagination", 
                System.Text.Json.JsonSerializer.Serialize(new { 
                    users.CurrentPage, users.TotalPages, users.PageSize, users.TotalCount 
                }));

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto userUpdateDto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            try 
            {
                var user = await _userService.UpdateUserAsync(id, userUpdateDto, currentUserId!, isAdmin);
                if (user == null) return NotFound();
                return Ok(user);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id, [FromQuery] bool hardDelete = false)
        {
            bool success;
            if (hardDelete)
            {
                success = await _userService.HardDeleteUserAsync(id);
            }
            else
            {
                success = await _userService.SoftDeleteUserAsync(id);
            }

            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPost("profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            try
            {
                var fileUrl = await _userService.UploadProfilePictureAsync(currentUserId!, file);
                return Ok(new { Url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}/roles")]
        public async Task<IActionResult> GetUserRoles(string id)
        {
            // Allow Admin or Self
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && currentUserId != id)
            {
                return Forbid();
            }

            var roles = await _userService.GetUserRolesAsync(id);
            return Ok(roles);
        }

        [HttpPost("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(string id, [FromBody] RoleAssignmentDto roleDto)
        {
            var success = await _userService.AssignRoleAsync(id, roleDto.RoleName);
            if (!success) return BadRequest("Failed to assign role. Role might not exist or user not found.");
            return Ok(new { Message = $"Role '{roleDto.RoleName}' assigned successfully." });
        }

        [HttpDelete("{id}/roles/{roleName}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole(string id, string roleName)
        {
            var success = await _userService.RemoveRoleAsync(id, roleName);
            if (!success) return BadRequest("Failed to remove role.");
            return Ok(new { Message = $"Role '{roleName}' removed successfully." });
        }
    }
}