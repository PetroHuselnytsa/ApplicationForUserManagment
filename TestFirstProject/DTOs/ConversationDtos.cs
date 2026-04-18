namespace TestFirstProject.DTOs
{
    // --- Conversations ---

    /// <summary>Request to start or retrieve an existing conversation with a target user.</summary>
    public record StartConversationRequest(Guid ParticipantId);

    /// <summary>Summary of a conversation for listing purposes.</summary>
    public record ConversationSummaryDto(
        Guid Id,
        Guid OtherUserId,
        string OtherUsername,
        string? LastMessageContent,
        DateTime? LastMessageAt,
        int UnreadCount
    );
}
