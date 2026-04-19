using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Common;
using TestFirstProject.DTOs.Messaging;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Manages conversations and messages between users.
    /// Handles conversation creation, message CRUD, pagination, and authorization checks.
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly PersonsContext _context;

        public MessageService(PersonsContext context)
        {
            _context = context;
        }

        public async Task<ConversationDto> StartOrGetConversationAsync(Guid currentUserId, Guid recipientId)
        {
            // Prevent messaging yourself
            if (currentUserId == recipientId)
                throw new BadRequestException("You cannot start a conversation with yourself.");

            // Validate recipient exists
            var recipient = await _context.Users.FindAsync(recipientId)
                ?? throw new NotFoundException("User", recipientId);

            // Check for existing conversation between these two users
            var existingConversation = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId)
                          && c.Participants.Any(p => p.UserId == recipientId))
                .FirstOrDefaultAsync();

            if (existingConversation != null)
                return MapToConversationDto(existingConversation, currentUserId);

            // Create new conversation
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
                UserId = recipientId
            });

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var created = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .FirstAsync(c => c.Id == conversation.Id);

            return MapToConversationDto(created, currentUserId);
        }

        public async Task<PagedResult<ConversationDto>> GetConversationsAsync(Guid currentUserId, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId))
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt);

            var totalCount = await query.CountAsync();

            var conversations = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calculate unread counts per conversation
            var conversationIds = conversations.Select(c => c.Id).ToList();
            var unreadCounts = await _context.Messages
                .Where(m => conversationIds.Contains(m.ConversationId)
                         && m.SenderId != currentUserId
                         && !m.IsRead)
                .GroupBy(m => m.ConversationId)
                .Select(g => new { ConversationId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ConversationId, x => x.Count);

            var items = conversations.Select(c =>
            {
                var dto = MapToConversationDto(c, currentUserId);
                dto.UnreadCount = unreadCounts.GetValueOrDefault(c.Id, 0);
                return dto;
            }).ToList();

            return new PagedResult<ConversationDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CursorPagedResult<MessageDto>> GetMessagesAsync(
            Guid currentUserId, Guid conversationId, string? cursor, int pageSize)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

            // Verify the user is a participant
            await ValidateParticipantAsync(currentUserId, conversationId);

            var query = _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .Where(m => !(m.IsDeletedBySender && m.SenderId == currentUserId));

            // Cursor-based pagination: cursor is the message ID; fetch messages older than that
            if (!string.IsNullOrEmpty(cursor) && Guid.TryParse(cursor, out var cursorId))
            {
                var cursorMessage = await _context.Messages.FindAsync(cursorId);
                if (cursorMessage != null)
                {
                    query = query.Where(m => m.CreatedAt < cursorMessage.CreatedAt
                        || (m.CreatedAt == cursorMessage.CreatedAt && m.Id.CompareTo(cursorId) < 0));
                }
            }

            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.Id)
                .Take(pageSize + 1)
                .ToListAsync();

            bool hasMore = messages.Count > pageSize;
            if (hasMore)
                messages = messages.Take(pageSize).ToList();

            var items = messages.Select(MapToMessageDto).ToList();

            return new CursorPagedResult<MessageDto>
            {
                Items = items,
                NextCursor = hasMore && items.Count > 0 ? items.Last().Id.ToString() : null,
                HasMore = hasMore
            };
        }

        public async Task<MessageDto> SendMessageAsync(Guid currentUserId, Guid conversationId, string content)
        {
            // Validate participation
            await ValidateParticipantAsync(currentUserId, conversationId);

            // Sanitize content: strip null bytes and enforce max length
            content = SanitizeContent(content);

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderId = currentUserId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);

            // Update conversation's last message timestamp
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.LastMessageAt = message.CreatedAt;
            }

            await _context.SaveChangesAsync();

            // Reload with sender info
            var savedMessage = await _context.Messages
                .Include(m => m.Sender)
                .FirstAsync(m => m.Id == message.Id);

            return MapToMessageDto(savedMessage);
        }

        public async Task MarkMessageAsReadAsync(Guid currentUserId, Guid conversationId, Guid messageId)
        {
            await ValidateParticipantAsync(currentUserId, conversationId);

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.ConversationId == conversationId)
                ?? throw new NotFoundException("Message", messageId);

            // Only the recipient (not the sender) can mark a message as read
            if (message.SenderId == currentUserId)
                throw new BadRequestException("You cannot mark your own message as read.");

            if (!message.IsRead)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteMessageAsync(Guid currentUserId, Guid conversationId, Guid messageId)
        {
            await ValidateParticipantAsync(currentUserId, conversationId);

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.ConversationId == conversationId)
                ?? throw new NotFoundException("Message", messageId);

            // Only the sender can soft-delete their own message
            if (message.SenderId != currentUserId)
                throw new ForbiddenException("You can only delete your own messages.");

            message.IsDeletedBySender = true;
            await _context.SaveChangesAsync();
        }

        public async Task<Guid> GetConversationRecipientAsync(Guid conversationId, Guid currentUserId)
        {
            var participant = await _context.ConversationParticipants
                .Where(cp => cp.ConversationId == conversationId && cp.UserId != currentUserId)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException("Conversation participant not found.");

            return participant.UserId;
        }

        /// <summary>
        /// Validates that the user is a participant in the given conversation.
        /// </summary>
        private async Task ValidateParticipantAsync(Guid userId, Guid conversationId)
        {
            bool isParticipant = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (!isParticipant)
                throw new ForbiddenException("You are not a participant in this conversation.");
        }

        /// <summary>
        /// Strip null bytes and enforce maximum content length.
        /// </summary>
        private static string SanitizeContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new BadRequestException("Message content cannot be empty.");

            // Remove null bytes
            content = content.Replace("\0", string.Empty);

            if (content.Length > 2000)
                content = content[..2000];

            return content;
        }

        /// <summary>
        /// Map a Conversation entity to a ConversationDto for the given user.
        /// </summary>
        private static ConversationDto MapToConversationDto(Conversation conversation, Guid currentUserId)
        {
            var otherParticipant = conversation.Participants
                .FirstOrDefault(p => p.UserId != currentUserId);

            var lastMessage = conversation.Messages
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            return new ConversationDto
            {
                Id = conversation.Id,
                OtherUserId = otherParticipant?.UserId ?? Guid.Empty,
                OtherUsername = otherParticipant?.User?.Username ?? "Unknown",
                LastMessageContent = lastMessage?.Content,
                LastMessageAt = lastMessage?.CreatedAt ?? conversation.LastMessageAt,
                UnreadCount = 0, // Will be populated separately
                CreatedAt = conversation.CreatedAt
            };
        }

        /// <summary>
        /// Map a Message entity to a MessageDto.
        /// </summary>
        private static MessageDto MapToMessageDto(Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderUsername = message.Sender?.Username ?? "Unknown",
                Content = message.Content,
                IsRead = message.IsRead,
                CreatedAt = message.CreatedAt,
                ReadAt = message.ReadAt
            };
        }
    }
}
