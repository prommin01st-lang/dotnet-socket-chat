using ChatBackend.Data;
using ChatBackend.Entities;
using ChatBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBackend.Services
{
    public class ConversationService : IConversationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConversationService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(string currentUserId)
        {
            var conversations = await _context.Conversations
                .Where(c => c.Participants.Any(p => p.ApplicationUserId == currentUserId))
                .Include(c => c.Participants)
                    .ThenInclude(p => p.ApplicationUser) 
                .Include(c => c.Messages.OrderByDescending(m => m.Timestamp)) 
                .ToListAsync();

            var conversationDtos = conversations
                .Select(convo => MapToConversationDto(convo, currentUserId)) 
                .ToList();

            var orderedDtos = conversationDtos
                .OrderByDescending(d => d.LastMessage?.Timestamp ?? d.CreatedAt)
                .ToList();

            return orderedDtos;
        }

        public async Task<IEnumerable<MessageDto>?> GetMessagesAsync(Guid conversationId, string currentUserId)
        {
            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.ApplicationUserId == currentUserId);

            if (!isParticipant)
            {
                return null; 
            }

            var messagesFromDb = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.Timestamp) 
                .Include(m => m.Sender) 
                .ToListAsync();

            var messages = messagesFromDb.Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                Timestamp = m.Timestamp,
                SenderId = m.SenderId,
                SenderFirstName = m.Sender?.FirstName ?? "Unknown",
                SenderLastName = m.Sender?.LastName ?? "User",
                SenderProfilePictureUrl = m.Sender?.ProfilePictureUrl,
                
                // (!!! FIX !!!)
                // (เพิ่มค่านี้เข้าไปใน DTO)
                ConversationId = m.ConversationId
            });

            return messages;
        }

        public async Task<ConversationDto> GetOrCreateOneToOneConversationAsync(string currentUserId, string otherUserId)
        {
            var otherUser = await _userManager.FindByIdAsync(otherUserId);
            if (otherUser == null)
            {
                throw new KeyNotFoundException("The other user could not be found.");
            }

            var existingConversation = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.ApplicationUser)
                .Include(c => c.Messages.OrderByDescending(m => m.Timestamp)) 
                .FirstOrDefaultAsync(c => 
                    c.Type == ConversationType.OneToOne && 
                    c.Participants.Count == 2 && 
                    c.Participants.Any(p => p.ApplicationUserId == currentUserId) && 
                    c.Participants.Any(p => p.ApplicationUserId == otherUserId));  

            if (existingConversation != null)
            {
                return MapToConversationDto(existingConversation, currentUserId); 
            }

            var newConversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = ConversationType.OneToOne,
                CreatedAt = DateTime.UtcNow
            };

            newConversation.Participants.Add(new ConversationParticipant
            {
                ApplicationUserId = currentUserId,
                ConversationId = newConversation.Id 
            });
            newConversation.Participants.Add(new ConversationParticipant
            {
                ApplicationUserId = otherUserId,
                ConversationId = newConversation.Id 
            });

            _context.Conversations.Add(newConversation);
            await _context.SaveChangesAsync();

            var createdConvo = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.ApplicationUser)
                .FirstAsync(c => c.Id == newConversation.Id);

            return MapToConversationDto(createdConvo, currentUserId); 
        }

        public async Task<bool> MarkConversationAsReadAsync(Guid conversationId, string currentUserId)
        {   
            var participant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.ApplicationUserId == currentUserId);
            
            if (participant == null) return false;

            participant.LastReadTimestamp = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private ConversationDto MapToConversationDto(Conversation convo, string currentUserId)
        {
            var myParticipation = convo.Participants
                .FirstOrDefault(p => p.ApplicationUserId == currentUserId);
            
            var lastRead = myParticipation?.LastReadTimestamp ?? DateTime.MinValue;
            var unreadCount = convo.Messages.Count(m => m.Timestamp > lastRead);

            var dto = new ConversationDto
            {
                Id = convo.Id,
                Type = convo.Type,
                CreatedAt = convo.CreatedAt,
                UnreadCount = unreadCount,
                Participants = convo.Participants
                    .Where(p => p.ApplicationUser != null) 
                    .Select(p => new ParticipantDto
                    {
                        Id = p.ApplicationUser.Id, 
                        Email = p.ApplicationUser.Email ?? "no-email@example.com",
                        FirstName = p.ApplicationUser.FirstName ?? "Unknown",
                        LastName = p.ApplicationUser.LastName ?? "User",
                        ProfilePictureUrl = p.ApplicationUser.ProfilePictureUrl
                    }).ToList()
            };

            var lastMessage = convo.Messages.FirstOrDefault();
            if (lastMessage != null)
            {
                var sender = convo.Participants
                    .FirstOrDefault(p => p.ApplicationUserId == lastMessage.SenderId)?
                    .ApplicationUser; 

                dto.LastMessage = new MessageDto
                {
                    Id = lastMessage.Id,
                    Content = lastMessage.Content,
                    Timestamp = lastMessage.Timestamp,
                    SenderId = lastMessage.SenderId,
                    SenderFirstName = sender?.FirstName ?? "Unknown",
                    SenderLastName = sender?.LastName ?? "User",
                    SenderProfilePictureUrl = sender?.ProfilePictureUrl,
                    ConversationId = lastMessage.ConversationId
                };
            }

            if (dto.Type == ConversationType.OneToOne)
            {
                var otherParticipant = dto.Participants.FirstOrDefault(p => p.Id != currentUserId);
                if (otherParticipant != null)
                {
                    dto.Name = $"{otherParticipant.FirstName} {otherParticipant.LastName}";
                    dto.ImageUrl = otherParticipant.ProfilePictureUrl;
                }
                else
                {
                    dto.Name = "Self Conversation"; 
                }
            }
            else
            {
                dto.Name = convo.Name;
            }

            return dto;
        }
    }
}