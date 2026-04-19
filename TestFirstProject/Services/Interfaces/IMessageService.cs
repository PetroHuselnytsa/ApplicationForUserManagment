using TestFirstProject.DTOs.Common;
using TestFirstProject.DTOs.Messaging;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Manages conversations and messages between users.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Start a new conversation or return existing one between two users.
        /// </summary>
        Task<ConversationDto> StartOrGetConversationAsync(Guid currentUserId, Guid recipientId);

        /// <summary>
        /// List all conversations for a user, ordered by most recent activity.
        /// </summary>
        Task<PagedResult<ConversationDto>> GetConversationsAsync(Guid currentUserId, int page, int pageSize);

        /// <summary>
        /// Get paginated message history for a conversation using cursor-based pagination.
        /// </summary>
        Task<CursorPagedResult<MessageDto>> GetMessagesAsync(Guid currentUserId, Guid conversationId, string? cursor, int pageSize);

        /// <summary>
        /// Send a message in a conversation.
        /// </summary>
        Task<MessageDto> SendMessageAsync(Guid currentUserId, Guid conversationId, string content);

        /// <summary>
        /// Mark a specific message as read.
        /// </summary>
        Task MarkMessageAsReadAsync(Guid currentUserId, Guid conversationId, Guid messageId);

        /// <summary>
        /// Soft-delete a message (from sender's perspective only).
        /// </summary>
        Task SoftDeleteMessageAsync(Guid currentUserId, Guid conversationId, Guid messageId);

        /// <summary>
        /// Get the recipient user ID for a conversation (the other participant).
        /// </summary>
        Task<Guid> GetConversationRecipientAsync(Guid conversationId, Guid currentUserId);
    }
}
