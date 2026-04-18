using TestFirstProject.Models;

namespace TestFirstProject.DTOs
{
    // -----------------------------------------------------------------------
    // Request DTOs
    // -----------------------------------------------------------------------

    /// <summary>
    /// Request to start or get an existing conversation with another user.
    /// </summary>
    public record StartConversationRequest(Guid RecipientUserId);

    /// <summary>
    /// Request to send a message within a conversation.
    /// </summary>
    public record SendMessageRequest(string Content);

    // -----------------------------------------------------------------------
    // Response DTOs
    // -----------------------------------------------------------------------

    /// <summary>
    /// Summary of a conversation for the list view.
    /// </summary>
    public record ConversationSummaryDto(
        Guid Id,
        ParticipantDto OtherUser,
        string? LastMessageContent,
        DateTime? LastMessageAt,
        int UnreadCount
    );

    /// <summary>
    /// Minimal user info for conversation participants.
    /// </summary>
    public record ParticipantDto(Guid Id, string Name, string Email);

    /// <summary>
    /// Full conversation details including participants.
    /// </summary>
    public record ConversationDto(
        Guid Id,
        DateTime CreatedAt,
        DateTime? LastMessageAt,
        IEnumerable<ParticipantDto> Participants
    );

    /// <summary>
    /// A single message in a conversation.
    /// </summary>
    public record MessageDto(
        Guid Id,
        Guid ConversationId,
        Guid SenderId,
        string SenderName,
        string Content,
        bool IsRead,
        DateTime CreatedAt,
        DateTime? ReadAt
    );

    /// <summary>
    /// Cursor-based paginated response for messages.
    /// </summary>
    public record PaginatedMessagesDto(
        IEnumerable<MessageDto> Messages,
        string? NextCursor,
        bool HasMore
    );
}
