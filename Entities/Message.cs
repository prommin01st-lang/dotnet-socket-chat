using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Namespace คือ .Entities
namespace ChatBackend.Entities
{
    /// <summary>
    /// Model สำหรับ "ข้อความ"
    /// </summary>
    public class Message
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(4000)] // จำกัดความยาวข้อความ
        public required string Content { get; set; } 

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // (Optional) เพิ่มสถานะ Read/Unread ได้ในอนาคต
        // public bool IsRead { get; set; } = false;


        // --- Foreign Keys ---

        /// <summary>
        /// FK ไปยัง ApplicationUser (ผู้ส่ง)
        /// </summary>
        [Required]
        public required string SenderId { get; set; } 

        /// <summary>
        /// FK ไปยัง Conversation (ห้องแชท)
        /// </summary>
        [Required]
        public required Guid ConversationId { get; set; } // <--- (ยืนยัน: 'required' ถูกต้องแล้ว)


        // --- Navigation Properties ---

        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; } = null!; 

        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; } = null!; 
    }
}