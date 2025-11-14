using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatBackend.Entities
{

    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        /// <summary>
        /// URL ของรูปโปรไฟล์
        /// </summary>
        public string? ProfilePictureUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public string? RefreshToken { get; set; }
        
        /// <summary>
        /// วันหมดอายุของ Refresh Token
        /// </summary>
        public DateTime? RefreshTokenExpiryTime { get; set; }



        // --- ความสัมพันธ์สำหรับระบบ Chat ---

        /// <summary>
        /// รายการ Conversation (ห้องแชท) ที่ User นี้เข้าร่วม
        /// (ผ่านตารางเชื่อม ConversationParticipant)
        /// </summary>
        public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();

        /// <summary>
        /// รายการข้อความทั้งหมดที่ User นี้เป็นคนส่ง
        /// </summary>
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
    }
}