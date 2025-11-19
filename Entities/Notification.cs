using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatBackend.Entities
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Message { get; set; }

        public NotificationType Type { get; set; } = NotificationType.Info;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- Optional: Link ไปยังสิ่งที่เกี่ยวข้อง ---
        public string? RelatedEntityId { get; set; } // เช่น ID ห้องแชท
        public string? RelatedEntityType { get; set; } // เช่น "Conversation"

        // --- Foreign Keys ---
        [Required]
        public required string UserId { get; set; } // แจ้งเตือนนี้ของใคร

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}