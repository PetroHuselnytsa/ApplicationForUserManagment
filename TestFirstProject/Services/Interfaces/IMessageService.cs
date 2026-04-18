using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Service responsible for all messaging operations including
    /// conversations, messages, and message read status.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Starts a new conversation or returns the existing one between two users.
        /// </summary>
        Task<ConversationDto> StartOrGetConversationAsync(Guid currentUserId, Guid recipientUserId);

        /// <summary>
        /// Returns all conversations for the current user with last message preview and unread count.
        /// </summary>
        Task<IEnumerable<ConversationSummaryDto>> GetConversationsAsync(Guid currentUserId);

        /// <summary>
        /// Returns paginated message history for a conversation using cursor-based pagination.
        /// </summary>
        Task<PaginatedMessagesDto> GetMessagesAsync(Guid currentUserId, Guid conversationId, string? cursor, int pageSize);

        /// <summary>
        /// Sends a message from the current user within a conversation.
        /// Returns the created message DTO.
        /// </summary>
        Task<MessageDto> SendMessageAsync(Guid currentUserId, Guid conversationId, string content);

        /// <summary>
        /// Marks a specific message as read.
        /// </summary>
        Task MarkMessageAsReadAsync(Guid currentUserId, Guid conversationId, Guid messageId);

        /// <summary>
        /// Soft-deletes a message from the sender's perspective.
        /// </summary>
        Task SoftDeleteMessageAsync(Guid currentUserId, Guid conversationId, Guid messageId);

        /// <summary>
        /// Gets the recipient user ID for a message in a conversation (used for real-time push).
        /// </summary>
        Task<Guid> GetRecipientUserIdAsync(Guid senderId, Guid conversationId);
    }
}
