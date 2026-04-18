using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Handles all messaging business logic: conversations, messages, read/unread status.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>Start or retrieve an existing conversation between two users.</summary>
        Task<ConversationSummaryDto> StartOrGetConversationAsync(Guid currentUserId, Guid participantId);

        /// <summary>List all conversations for the current user with last message preview and unread count.</summary>
        Task<IReadOnlyList<ConversationSummaryDto>> GetConversationsAsync(Guid currentUserId);

        /// <summary>Get paginated message history using cursor-based pagination.</summary>
        Task<CursorPagedResult<MessageDto>> GetMessagesAsync(Guid currentUserId, Guid conversationId, string? cursor, int pageSize = 50);

        /// <summary>Send a message in a conversation.</summary>
        Task<MessageDto> SendMessageAsync(Guid currentUserId, Guid conversationId, string content);

        /// <summary>Mark a specific message as read.</summary>
        Task MarkMessageAsReadAsync(Guid currentUserId, Guid conversationId, Guid messageId);

        /// <summary>Soft delete a message (sender deletes from their side only).</summary>
        Task DeleteMessageAsync(Guid currentUserId, Guid conversationId, Guid messageId);

        /// <summary>Get unread message count for a conversation.</summary>
        Task<int> GetUnreadCountAsync(Guid currentUserId, Guid conversationId);
    }
}
