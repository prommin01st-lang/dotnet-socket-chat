using ChatBackend.Entities;
using ChatBackend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBackend.Services
{
    public interface INotificationService
    {
        // ดึงแจ้งเตือนทั้งหมดของ User
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId, int page, int pageSize);
        
        // สร้างแจ้งเตือน (และส่ง Realtime)
        Task<NotificationDto> CreateNotificationAsync(string userId, string title, string? message, NotificationType type, string? relatedEntityId = null, string? relatedEntityType = null);
        
        // อ่านแจ้งเตือน 1 อัน
        Task MarkAsReadAsync(Guid notificationId);
        
        // อ่านแจ้งเตือนทั้งหมด
        Task MarkAllAsReadAsync(string userId);

        Task DeleteNotificationAsync(Guid notificationId);
    }
}