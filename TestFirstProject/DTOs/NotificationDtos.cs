using TestFirstProject.Models;

namespace TestFirstProject.DTOs
{
    // -----------------------------------------------------------------------
    // Response DTOs
    // -----------------------------------------------------------------------

    /// <summary>
    /// A single notification in the list.
    /// </summary>
    public record NotificationDto(
        Guid Id,
        string Type,
        string Title,
        string Body,
        bool IsRead,
        DateTime CreatedAt,
        DateTime? ReadAt,
        string? ReferenceId
    );

    /// <summary>
    /// Paginated response for notifications.
    /// </summary>
    public record PaginatedNotificationsDto(
        IEnumerable<NotificationDto> Notifications,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages
    );

    /// <summary>
    /// Request to update notification preferences.
    /// </summary>
    public record UpdateNotificationPreferenceRequest(NotificationType NotificationType, bool IsEnabled);

    /// <summary>
    /// Notification preference response.
    /// </summary>
    public record NotificationPreferenceDto(
        Guid Id,
        string NotificationType,
        bool IsEnabled
    );
}
