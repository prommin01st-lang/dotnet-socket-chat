using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Namespace คือ .Entities
namespace ChatBackend.Entities
{
    /// <summary>
    /// ตารางเชื่อม (Join Table) สำหรับความสัมพันธ์ Many-to-Many
    /// ระหว่าง ApplicationUser และ Conversation
    /// </summary>
    public class ConversationParticipant
    {
        // --- Composite Primary Key (กำหนดใน DbContext) ---

        /// <summary>
        /// FK ไปยัง ApplicationUser
        /// </summary>
        [Required]
        public required string ApplicationUserId { get; set; } 

        /// <summary>
        /// FK ไปยัง Conversation
        /// </summary>
        [Required]
        public required Guid ConversationId { get; set; } // <--- (ยืนยัน: 'required' ถูกต้องแล้ว)
        
        // (Optional) เพิ่ม field เช่น Role ในห้อง (Admin, Member) หรือ Nickname ได้
        // public DateTime JoinedAt { get; set; } = DateTime.UtcNow;


        // --- Navigation Properties ---

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; } = null!; 

        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; } = null!; 
    }
}