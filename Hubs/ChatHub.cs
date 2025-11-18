using ChatBackend.Data;
using ChatBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatBackend.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            if (userId is null)
            {
                await base.OnConnectedAsync();
                return;
            }

            var conversationIds = await _context.ConversationParticipants
                .Where(cp => cp.ApplicationUserId == userId)
                .Select(cp => cp.ConversationId.ToString())
                .ToListAsync();

            foreach (var convoId in conversationIds)
                await Groups.AddToGroupAsync(Context.ConnectionId, convoId);

            var debugMessage = new MessageDto
            {
                Id = Guid.NewGuid(),
                Content = $"HUB_V2_CONNECTED. Subscribed to {conversationIds.Count} groups.",
                Timestamp = DateTime.UtcNow,
                SenderId = "SYSTEM",
                SenderFirstName = "System",
                SenderLastName = "Debug",
                ConversationId = Guid.Empty
            };

            await Clients.Caller.SendAsync("ReceiveMessage", debugMessage);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
