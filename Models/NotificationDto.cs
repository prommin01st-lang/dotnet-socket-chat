using ChatBackend.Entities;
using System;

namespace ChatBackend.Models
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string Type { get; set; } = "Info"; // ส่งเป็น String ไปใช้ง่ายกว่า
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
    }
}