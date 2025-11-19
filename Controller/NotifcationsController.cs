using ChatBackend.Models;
using ChatBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChatBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ต้อง Login
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: api/notifications
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
            return Ok(notifications);
        } 

        // POST: api/notifications/{id}/read
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { message = "Marked as read" });
        }

        // POST: api/notifications/read-all
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { message = "All marked as read" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            return Ok(new { message = "Deleted" });
        }
    }
}