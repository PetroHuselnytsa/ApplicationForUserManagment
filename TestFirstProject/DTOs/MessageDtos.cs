namespace TestFirstProject.DTOs
{
    // --- Messages ---

    public record SendMessageRequest(string Content);

    public record MessageDto(
        Guid Id,
        Guid SenderId,
        string SenderUsername,
        string Content,
        bool IsRead,
        DateTime CreatedAt,
        DateTime? ReadAt
    );

    /// <summary>Cursor-based paginated result for messages.</summary>
    public record CursorPagedResult<T>(
        IReadOnlyList<T> Items,
        string? NextCursor,
        bool HasMore
    );
}
