using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Implementation of IMessageService handling conversation and message operations.
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly PersonsContext _context;
        private readonly ILogger<MessageService> _logger;

        // Maximum message content length
        private const int MaxMessageLength = 4000;

        public MessageService(PersonsContext context, ILogger<MessageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ConversationDto> StartOrGetConversationAsync(Guid currentUserId, Guid recipientUserId)
        {
            // Validate: cannot start conversation with yourself
            if (currentUserId == recipientUserId)
            {
                throw new ValidationException("Cannot start a conversation with yourself.");
            }

            // Validate: recipient exists
            var recipientExists = await _context.Users.AnyAsync(u => u.Id == recipientUserId);
            if (!recipientExists)
            {
                throw new NotFoundException("Recipient user not found.");
            }

            // Check if conversation already exists between these two users
            var existingConversation = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId)
                          && c.Participants.Any(p => p.UserId == recipientUserId))
                .FirstOrDefaultAsync();

            if (existingConversation != null)
            {
                return MapToConversationDto(existingConversation);
            }

            // Create new conversation
            var conversation = new Conversation();
            _context.Conversations.Add(conversation);

            // Add participants
            var participant1 = new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = currentUserId
            };
            var participant2 = new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = recipientUserId
            };

            _context.ConversationParticipants.Add(participant1);
            _context.ConversationParticipants.Add(participant2);

            await _context.SaveChangesAsync();

            _logger.LogInformation("New conversation {ConversationId} created between {User1} and {User2}",
                conversation.Id, currentUserId, recipientUserId);

            // Reload with participants and user info
            var created = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .FirstAsync(c => c.Id == conversation.Id);

            return MapToConversationDto(created);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ConversationSummaryDto>> GetConversationsAsync(Guid currentUserId)
        {
            // Fetch unread counts in a single query to avoid N+1
            var conversationIds = await _context.ConversationParticipants
                .Where(cp => cp.UserId == currentUserId)
                .Select(cp => cp.ConversationId)
                .ToListAsync();

            var unreadCounts = await _context.Messages
                .Where(m => conversationIds.Contains(m.ConversationId)
                         && m.SenderId != currentUserId
                         && !m.IsRead
                         && !m.IsDeletedByRecipient)
                .GroupBy(m => m.ConversationId)
                .Select(g => new { ConversationId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ConversationId, x => x.Count);

            var conversations = await _context.Conversations
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .Where(c => conversationIds.Contains(c.Id))
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .ToListAsync();

            var result = new List<ConversationSummaryDto>();

            foreach (var conv in conversations)
            {
                var otherParticipant = conv.Participants.FirstOrDefault(p => p.UserId != currentUserId);
                if (otherParticipant == null) continue;

                var lastMessage = conv.Messages.FirstOrDefault();

                string? lastMessageContent = null;
                if (lastMessage != null)
                {
                    bool isDeletedForCurrentUser = lastMessage.SenderId == currentUserId
                        ? lastMessage.IsDeletedBySender
                        : lastMessage.IsDeletedByRecipient;

                    lastMessageContent = isDeletedForCurrentUser ? null : lastMessage.Content;
                }

                unreadCounts.TryGetValue(conv.Id, out var unreadCount);

                result.Add(new ConversationSummaryDto(
                    Id: conv.Id,
                    OtherUser: new ParticipantDto(
                        otherParticipant.User.Id,
                        otherParticipant.User.Name,
                        otherParticipant.User.Email),
                    LastMessageContent: lastMessageContent,
                    LastMessageAt: conv.LastMessageAt,
                    UnreadCount: unreadCount
                ));
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<PaginatedMessagesDto> GetMessagesAsync(
            Guid currentUserId, Guid conversationId, string? cursor, int pageSize)
        {
            // Validate user is participant
            await ValidateParticipantAsync(currentUserId, conversationId);

            // Clamp page size
            pageSize = Math.Clamp(pageSize, 1, 100);

            // Build query — cursor-based pagination using CreatedAt timestamp
            var query = _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId);

            // Apply cursor filter (cursor is a base64-encoded datetime)
            if (!string.IsNullOrEmpty(cursor))
            {
                try
                {
                    var cursorBytes = Convert.FromBase64String(cursor);
                    var cursorDate = DateTime.Parse(
                        System.Text.Encoding.UTF8.GetString(cursorBytes)).ToUniversalTime();
                    query = query.Where(m => m.CreatedAt < cursorDate);
                }
                catch (Exception)
                {
                    throw new ValidationException("Invalid cursor format.");
                }
            }

            // Filter out soft-deleted messages for current user
            query = query.Where(m =>
                (m.SenderId == currentUserId && !m.IsDeletedBySender) ||
                (m.SenderId != currentUserId && !m.IsDeletedByRecipient));

            // Order by newest first and take pageSize + 1 to determine if there are more
            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .Take(pageSize + 1)
                .ToListAsync();

            bool hasMore = messages.Count > pageSize;
            if (hasMore)
            {
                messages = messages.Take(pageSize).ToList();
            }

            // Build next cursor from the last message's CreatedAt
            string? nextCursor = null;
            if (hasMore && messages.Count > 0)
            {
                var lastDate = messages.Last().CreatedAt.ToString("O");
                nextCursor = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(lastDate));
            }

            var messageDtos = messages.Select(m => new MessageDto(
                Id: m.Id,
                ConversationId: m.ConversationId,
                SenderId: m.SenderId,
                SenderName: m.Sender.Name,
                Content: m.Content,
                IsRead: m.IsRead,
                CreatedAt: m.CreatedAt,
                ReadAt: m.ReadAt
            )).ToList();

            return new PaginatedMessagesDto(
                Messages: messageDtos,
                NextCursor: nextCursor,
                HasMore: hasMore
            );
        }

        /// <inheritdoc />
        public async Task<MessageDto> SendMessageAsync(Guid currentUserId, Guid conversationId, string content)
        {
            // Validate participant and load sender name in a single query
            var participant = await _context.ConversationParticipants
                .Include(cp => cp.User)
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == currentUserId);

            if (participant == null)
                throw new ForbiddenException("You are not a participant of this conversation.");

            ValidateMessageContent(content);

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = currentUserId,
                Content = content.Trim()
            };

            _context.Messages.Add(message);

            // Update conversation's last message timestamp — conversation is already tracked via participant
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.LastMessageAt = message.CreatedAt;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Message {MessageId} sent in conversation {ConversationId} by user {UserId}",
                message.Id, conversationId, currentUserId);

            return new MessageDto(
                Id: message.Id,
                ConversationId: message.ConversationId,
                SenderId: message.SenderId,
                SenderName: participant.User.Name,
                Content: message.Content,
                IsRead: message.IsRead,
                CreatedAt: message.CreatedAt,
                ReadAt: message.ReadAt
            );
        }

        /// <inheritdoc />
        public async Task MarkMessageAsReadAsync(Guid currentUserId, Guid conversationId, Guid messageId)
        {
            // Validate user is participant
            await ValidateParticipantAsync(currentUserId, conversationId);

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.ConversationId == conversationId);

            if (message == null)
            {
                throw new NotFoundException("Message not found.");
            }

            // Only the recipient (non-sender) can mark a message as read
            if (message.SenderId == currentUserId)
            {
                throw new ValidationException("Cannot mark your own message as read.");
            }

            if (!message.IsRead)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task SoftDeleteMessageAsync(Guid currentUserId, Guid conversationId, Guid messageId)
        {
            // Validate user is participant
            await ValidateParticipantAsync(currentUserId, conversationId);

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.ConversationId == conversationId);

            if (message == null)
            {
                throw new NotFoundException("Message not found.");
            }

            // Sender deletes from their side only
            if (message.SenderId == currentUserId)
            {
                message.IsDeletedBySender = true;
            }
            else
            {
                message.IsDeletedByRecipient = true;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Message {MessageId} soft-deleted by user {UserId}", messageId, currentUserId);
        }

        /// <inheritdoc />
        public async Task<Guid> GetRecipientUserIdAsync(Guid senderId, Guid conversationId)
        {
            var recipientParticipant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId != senderId);

            if (recipientParticipant == null)
            {
                throw new NotFoundException("Recipient not found in conversation.");
            }

            return recipientParticipant.UserId;
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Validates that the user is a participant of the conversation.
        /// </summary>
        private async Task ValidateParticipantAsync(Guid userId, Guid conversationId)
        {
            var isParticipant = await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (!isParticipant)
            {
                throw new ForbiddenException("You are not a participant of this conversation.");
            }
        }

        /// <summary>
        /// Validates and sanitizes message content.
        /// </summary>
        private static void ValidateMessageContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ValidationException("Message content cannot be empty.");
            }

            if (content.Length > MaxMessageLength)
            {
                throw new ValidationException($"Message content cannot exceed {MaxMessageLength} characters.");
            }

            // Check for null bytes
            if (content.Contains('\0'))
            {
                throw new ValidationException("Message content contains invalid characters.");
            }
        }

        /// <summary>
        /// Maps a Conversation entity to a ConversationDto.
        /// </summary>
        private static ConversationDto MapToConversationDto(Conversation conversation)
        {
            return new ConversationDto(
                Id: conversation.Id,
                CreatedAt: conversation.CreatedAt,
                LastMessageAt: conversation.LastMessageAt,
                Participants: conversation.Participants.Select(p => new ParticipantDto(
                    Id: p.User.Id,
                    Name: p.User.Name,
                    Email: p.User.Email
                ))
            );
        }
    }
}
