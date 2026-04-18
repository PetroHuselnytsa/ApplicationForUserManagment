using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Events;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Services.Interfaces;
using MediatR;

namespace TestFirstProject.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly PersonsContext _context;
        private readonly IMediator _mediator;

        public MessageService(PersonsContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<ConversationSummaryDto> StartOrGetConversationAsync(Guid currentUserId, Guid participantId)
        {
            if (currentUserId == participantId)
                throw new ValidationException("You cannot start a conversation with yourself.");

            var targetUser = await _context.AppUsers.FindAsync(participantId)
                ?? throw new NotFoundException("Target user not found.");

            var existingConversation = await ConversationsWithParticipants()
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId)
                          && c.Participants.Any(p => p.UserId == participantId))
                .FirstOrDefaultAsync();

            if (existingConversation != null)
            {
                int unreadCount = await GetUnreadCountQueryAsync(existingConversation.Id, currentUserId);
                return MapToConversationSummary(existingConversation, currentUserId, unreadCount);
            }

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            conversation.Participants.Add(new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = currentUserId
            });

            conversation.Participants.Add(new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = participantId
            });

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var loaded = await ConversationsWithParticipants()
                .FirstAsync(c => c.Id == conversation.Id);

            return MapToConversationSummary(loaded, currentUserId, 0);
        }

        public async Task<IReadOnlyList<ConversationSummaryDto>> GetConversationsAsync(Guid currentUserId)
        {
            var conversations = await ConversationsWithParticipants()
                .AsNoTracking()
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId))
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .ToListAsync();

            if (conversations.Count == 0)
                return Array.Empty<ConversationSummaryDto>();

            // Batch unread counts in a single query instead of N+1
            var conversationIds = conversations.Select(c => c.Id).ToList();
            var unreadCounts = await _context.Messages
                .Where(m => conversationIds.Contains(m.ConversationId)
                          && m.SenderId != currentUserId
                          && !m.IsRead
                          && !m.IsDeletedByRecipient)
                .GroupBy(m => m.ConversationId)
                .Select(g => new { ConversationId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ConversationId, x => x.Count);

            return conversations.Select(conv =>
            {
                unreadCounts.TryGetValue(conv.Id, out int unreadCount);
                return MapToConversationSummary(conv, currentUserId, unreadCount);
            }).ToList();
        }

        public async Task<CursorPagedResult<MessageDto>> GetMessagesAsync(
            Guid currentUserId, Guid conversationId, string? cursor, int pageSize = 50)
        {
            await EnsureParticipantAsync(currentUserId, conversationId);

            pageSize = Math.Clamp(pageSize, 1, 100);

            IQueryable<Message> query = _context.Messages
                .AsNoTracking()
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .Where(m => (m.SenderId == currentUserId && !m.IsDeletedBySender)
                         || (m.SenderId != currentUserId && !m.IsDeletedByRecipient));

            if (!string.IsNullOrEmpty(cursor))
            {
                try
                {
                    var decodedBytes = Convert.FromBase64String(cursor);
                    var cursorValue = new DateTime(BitConverter.ToInt64(decodedBytes, 0), DateTimeKind.Utc);
                    query = query.Where(m => m.CreatedAt < cursorValue);
                }
                catch (FormatException)
                {
                    throw new ValidationException("Invalid cursor format.");
                }
            }

            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .Take(pageSize + 1)
                .ToListAsync();

            bool hasMore = messages.Count > pageSize;
            if (hasMore)
            {
                messages = messages.Take(pageSize).ToList();
            }

            string? nextCursor = null;
            if (hasMore && messages.Count > 0)
            {
                var lastItem = messages[^1];
                nextCursor = Convert.ToBase64String(BitConverter.GetBytes(lastItem.CreatedAt.Ticks));
            }

            var items = messages.Select(MapToMessageDto).ToList();
            return new CursorPagedResult<MessageDto>(items, nextCursor, hasMore);
        }

        public async Task<MessageDto> SendMessageAsync(Guid currentUserId, Guid conversationId, string content)
        {
            ValidateMessageContent(content);

            // Single query to get all participants and verify membership
            var participants = await _context.ConversationParticipants
                .Include(cp => cp.User)
                .Where(cp => cp.ConversationId == conversationId)
                .ToListAsync();

            var currentParticipant = participants.FirstOrDefault(cp => cp.UserId == currentUserId)
                ?? throw new ForbiddenException("You are not a participant of this conversation.");

            var recipientParticipant = participants.FirstOrDefault(cp => cp.UserId != currentUserId)
                ?? throw new NotFoundException("Conversation participant not found.");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderId = currentUserId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);

            // Update LastMessageAt without an extra FindAsync — attach and set
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.LastMessageAt = message.CreatedAt;
            }

            await _context.SaveChangesAsync();

            // Set navigation property for mapping
            message.Sender = currentParticipant.User;

            var messageDto = MapToMessageDto(message);

            await _mediator.Publish(new MessageReceivedEvent(
                messageDto,
                conversationId,
                recipientParticipant.UserId,
                currentParticipant.User.Username
            ));

            return messageDto;
        }

        public async Task MarkMessageAsReadAsync(Guid currentUserId, Guid conversationId, Guid messageId)
        {
            await EnsureParticipantAsync(currentUserId, conversationId);

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.ConversationId == conversationId)
                ?? throw new NotFoundException("Message not found.");

            if (message.SenderId == currentUserId)
                throw new ValidationException("You cannot mark your own message as read.");

            if (!message.IsRead)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteMessageAsync(Guid currentUserId, Guid conversationId, Guid messageId)
        {
            await EnsureParticipantAsync(currentUserId, conversationId);

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.ConversationId == conversationId)
                ?? throw new NotFoundException("Message not found.");

            if (message.SenderId == currentUserId)
                message.IsDeletedBySender = true;
            else
                message.IsDeletedByRecipient = true;

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid currentUserId, Guid conversationId)
        {
            await EnsureParticipantAsync(currentUserId, conversationId);
            return await GetUnreadCountQueryAsync(conversationId, currentUserId);
        }

        // --- Private helpers ---

        private async Task EnsureParticipantAsync(Guid userId, Guid conversationId)
        {
            bool isParticipant = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (!isParticipant)
                throw new ForbiddenException("You are not a participant of this conversation.");
        }

        private async Task<int> GetUnreadCountQueryAsync(Guid conversationId, Guid currentUserId)
        {
            return await _context.Messages
                .CountAsync(m => m.ConversationId == conversationId
                              && m.SenderId != currentUserId
                              && !m.IsRead
                              && !m.IsDeletedByRecipient);
        }

        private static void ValidateMessageContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ValidationException("Message content cannot be empty.");

            if (content.Length > 4000)
                throw new ValidationException("Message content cannot exceed 4000 characters.");

            if (content.Contains('\0'))
                throw new ValidationException("Message content contains invalid characters.");
        }

        private IQueryable<Conversation> ConversationsWithParticipants()
        {
            return _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User);
        }

        private static MessageDto MapToMessageDto(Message m)
        {
            return new MessageDto(
                m.Id,
                m.SenderId,
                m.Sender.Username,
                m.Content,
                m.IsRead,
                m.CreatedAt,
                m.ReadAt
            );
        }

        private static ConversationSummaryDto MapToConversationSummary(
            Conversation conversation, Guid currentUserId, int unreadCount)
        {
            var otherParticipant = conversation.Participants.FirstOrDefault(p => p.UserId != currentUserId);
            var lastMessage = conversation.Messages.FirstOrDefault();

            return new ConversationSummaryDto(
                conversation.Id,
                otherParticipant?.UserId ?? Guid.Empty,
                otherParticipant?.User?.Username ?? "Unknown",
                lastMessage?.Content,
                lastMessage?.CreatedAt,
                unreadCount
            );
        }
    }
}
