using ChatBackend.Entities; // (ต้องการ ConversationType)
using System;
using System.Collections.Generic;

// Namespace คือ .Models
namespace ChatBackend.Models
{
    public class ConversationDto
    {
        public Guid Id { get; set; }
        public ConversationType Type { get; set; }
        
        /// <summary>
        /// ชื่อห้อง (สำหรับ Group Chat)
        /// หรือ ชื่อของ "คู่สนทนา" (สำหรับ OneToOne)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// รูปภาพห้อง (สำหรับ Group Chat)
        /// หรือ รูปของ "คู่สนทนา" (สำหรับ OneToOne)
        /// </summary>
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// รายชื่อผู้เข้าร่วมทั้งหมดในห้องนี้
        /// </summary>
        public List<ParticipantDto> Participants { get; set; } = new List<ParticipantDto>();

        /// <summary>
        /// ข้อความล่าสุด (สำหรับแสดง Preview)
        /// </summary>
        public MessageDto? LastMessage { get; set; }
    }
}