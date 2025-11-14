using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatBackend.Entities
{
    /// <summary>
    /// Model สำหรับ "ห้องแชท" (Conversation)
    /// </summary>
    public class Conversation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// ประเภทของห้องแชท (OneToOne หรือ Group)
        /// </summary>
        [Required]
        public ConversationType Type { get; set; }

        /// <summary>
        /// (Optional) ชื่อห้องแชท, ใช้สำหรับ Group Chat
        /// </summary>
        [StringLength(255)]
        public string? Name { get; set; }
        
        // (Optional) ไอคอนห้องแชทสำหรับ Group
        // public string? GroupImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // --- Navigation Properties ---

        /// <summary>
        /// ข้อความทั้งหมดในห้องแชทนี้
        /// </summary>
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

        /// <summary>
        /// ผู้เข้าร่วมทั้งหมดในห้องแชทนี้
        /// (ผ่านตารางเชื่อม ConversationParticipant)
        /// </summary>
        public virtual ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    }
}