using System;
using System.ComponentModel.DataAnnotations;

// Namespace คือ .Models
namespace ChatBackend.Models
{
    /// <summary>
    /// DTO (Model) สำหรับ "รับ" ข้อความใหม่จาก Client
    /// (POST /api/messages)
    /// </summary>
    public class CreateMessageDto
    {
        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public required string Content { get; set; }
    }
}