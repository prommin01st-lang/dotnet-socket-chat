using ChatBackend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBackend.Services
{
    /// <summary>
    /// Interface สำหรับจัดการ Business Logic ของระบบ Chat
    /// </summary>
    public interface IConversationService
    {
        /// <summary>
        /// (GET /conversations)
        /// ดึงห้องแชททั้งหมด (พร้อมข้อความล่าสุด) สำหรับ User ที่ Login อยู่
        /// </summary>
        Task<IEnumerable<ConversationDto>> GetConversationsAsync(string currentUserId);

        /// <summary>
        /// (GET /conversations/{id}/messages)
        /// ดึงข้อความทั้งหมดในห้องแชท (Service จะตรวจสอบสิทธิ์ให้ด้วย)
        /// </summary>
        Task<IEnumerable<MessageDto>?> GetMessagesAsync(Guid conversationId, string currentUserId);

        /// <summary>
        /// (POST /conversations/onetoone/{otherUserId})
        /// ค้นหา หรือ สร้าง ห้องแชท 1-1
        /// </summary>
        Task<ConversationDto> GetOrCreateOneToOneConversationAsync(string currentUserId, string otherUserId);

        // Make Conversation As Read 
        Task<bool> MarkConversationAsReadAsync(Guid conversationId, string currentUserId);

    }
}