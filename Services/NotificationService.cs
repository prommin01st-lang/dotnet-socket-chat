using ChatBackend.Data;
using ChatBackend.Entities;
using ChatBackend.Hubs;
using ChatBackend.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBackend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        // เรา Inject IHubContext เพื่อส่ง Realtime SignalR
        private readonly IHubContext<ChatHub> _hubContext;

        public NotificationService(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

         public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId, int page, int pageSize)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type.ToString(),
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    RelatedEntityId = n.RelatedEntityId,
                    RelatedEntityType = n.RelatedEntityType
                })
                .ToListAsync();
        }

        public async Task<NotificationDto> CreateNotificationAsync(string userId, string title, string? message, NotificationType type, string? relatedEntityId = null, string? relatedEntityType = null)
        {
            // 1. บันทึกลง Database
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // 2. แปลงเป็น DTO
            var dto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type.ToString(),
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                RelatedEntityId = notification.RelatedEntityId,
                RelatedEntityType = notification.RelatedEntityType
            };

            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", dto);

            return dto;
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (notifications.Any())
            {
                foreach (var n in notifications)
                {
                    n.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteNotificationAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }
    }
}