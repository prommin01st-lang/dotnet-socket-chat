using ChatBackend.Data;
using ChatBackend.Entities;
using ChatBackend.Models;
using ChatBackend.Services; // <-- (1) เพิ่ม using Service
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ผู้ใช้ต้อง Login เท่านั้นถึงจะเรียก Controller นี้ได้
    public class ConversationsController : ControllerBase
    {
        // (2) Controller จะ "ผอม" ลง
        // ลบ DbContext และ UserManager ออก
        private readonly IConversationService _conversationService;
        private readonly ILogger<ConversationsController> _logger;

        public ConversationsController(
            IConversationService conversationService,
            ILogger<ConversationsController> logger)
        {
            _conversationService = conversationService;
            _logger = logger;
        }

        // --- 1. Endpoint: [GET] /api/conversations ---
        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            try
            {
                // (3) ย้าย Logic ทั้งหมดไปให้ Service ทำ
                var conversationDtos = await _conversationService.GetConversationsAsync(currentUserId);
                return Ok(conversationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations for user {UserId}", currentUserId);
                return StatusCode(500, "Internal server error");
            }
        }

        // --- 2. Endpoint: [GET] /api/conversations/{id}/messages ---
        [HttpGet("{id}/messages")]
        public async Task<IActionResult> GetMessages(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            try
            {
                // (4) Service จะจัดการเรื่องการดึงข้อมูล และ ตรวจสอบสิทธิ์ (Authorization)
                var messages = await _conversationService.GetMessagesAsync(id, currentUserId);

                if (messages == null)
                {
                    // ถ้า Service คืนค่า null, แปลว่า User ไม่มีสิทธิ์ในห้องแชทนี้
                    return Forbid("You are not authorized to view messages in this conversation.");
                }

                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for conversation {ConversationId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // --- 3. Endpoint: [POST] /api/conversations/onetoone/{otherUserId} ---
        // (*** นี่คือ Endpoint ใหม่ (จาก plan) ***)
        [HttpPost("onetoone/{otherUserId}")]
        public async Task<IActionResult> GetOrCreateOneToOneConversation(string otherUserId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            if (currentUserId == otherUserId)
            {
                return BadRequest("Cannot create a conversation with yourself.");
            }

            try
            {
                // (5) Service จะจัดการ Logic ที่ซับซ้อน (ค้นหา หรือ สร้างใหม่)
                var conversationDto = await _conversationService.GetOrCreateOneToOneConversationAsync(currentUserId, otherUserId);
                
                // (Optional) เราสามารถคืนค่า 201 Created ถ้ามีการสร้างใหม่
                // แต่เพื่อความง่าย, คืน 200 OK ทั้งสองกรณี
                return Ok(conversationDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message); // ถ้า otherUserId ไม่มีอยู่จริง
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation between {UserId1} and {UserId2}", currentUserId, otherUserId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}