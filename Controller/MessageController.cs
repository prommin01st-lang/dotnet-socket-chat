using ChatBackend.Data;
using ChatBackend.Entities;
using ChatBackend.Hubs;
using ChatBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<ChatHub> hubContext) 
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] CreateMessageDto createMessageDto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == createMessageDto.ConversationId && 
                               cp.ApplicationUserId == currentUserId);

            if (!isParticipant)
            {
                return Forbid("You are not a participant in this conversation.");
            }

            var sender = await _userManager.FindByIdAsync(currentUserId);
            if (sender == null)
            {
                return NotFound("Sender not found.");
            }

            var message = new Message
            {
                Id = Guid.NewGuid(),
                Content = createMessageDto.Content,
                Timestamp = DateTime.UtcNow,
                SenderId = currentUserId,
                ConversationId = createMessageDto.ConversationId
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageDto = new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                Timestamp = message.Timestamp,
                SenderId = message.SenderId,
                SenderFirstName = sender.FirstName ?? "Unknown",
                SenderLastName = sender.LastName ?? "User",
                SenderProfilePictureUrl = sender.ProfilePictureUrl,
                ConversationId = message.ConversationId
            };

            // (!!! FIX !!!)
            // (วิธีแก้ Bug ที่ "อีกคน" (ผู้รับ) ไม่ได้รับข้อความ)
            
            // 1. (ใหม่) ค้นหา "User ID" ของ "ทุกคน" (All) ที่อยู่ในห้องแชทนี้
            var participantUserIds = await _context.ConversationParticipants
                .Where(p => p.ConversationId == message.ConversationId)
                .Select(p => p.ApplicationUserId)
                .ToListAsync();

            // 2. (ใหม่) "Push" (ผลัก) ข้อความนี้ ไปยัง "User ID" เหล่านั้น "โดยตรง"
            await _hubContext.Clients
                .Users(participantUserIds) 
                .SendAsync("ReceiveMessage", messageDto); 
            
            // (วิธีเก่า (ที่ Bug): .Group(message.ConversationId.ToString()))

            return CreatedAtAction(nameof(SendMessage), new { id = message.Id }, messageDto);
        }
    }
}