namespace TestFirstProject.DTOs
{
    // --- Notifications ---

    public record NotificationDto(
        Guid Id,
        string Type,
        string Title,
        string Body,
        bool IsRead,
        DateTime CreatedAt,
        DateTime? ReadAt
    );

    public record PagedResult<T>(
        IReadOnlyList<T> Items,
        int TotalCount,
        int Page,
        int PageSize
    );

    public record UpdateNotificationPreferenceRequest(
        bool NewMessageEnabled,
        bool RoleChangedEnabled,
        bool SystemAlertEnabled,
        bool UserMentionedEnabled
    );
}
