using TestFirstProject.DTOs;
using TestFirstProject.Models;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Service responsible for all notification operations including
    /// creation, retrieval, read status, and preferences.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Creates a notification for a specific user. Respects user notification preferences.
        /// Returns the created notification DTO, or null if the user has disabled this notification type.
        /// </summary>
        Task<NotificationDto?> CreateNotificationAsync(
            Guid userId,
            NotificationType type,
            string title,
            string body,
            string? referenceId = null);

        /// <summary>
        /// Returns a paginated list of notifications for the current user.
        /// </summary>
        Task<PaginatedNotificationsDto> GetNotificationsAsync(Guid userId, int page, int pageSize);

        /// <summary>
        /// Marks a single notification as read.
        /// </summary>
        Task MarkAsReadAsync(Guid userId, Guid notificationId);

        /// <summary>
        /// Marks all notifications as read for the current user.
        /// </summary>
        Task MarkAllAsReadAsync(Guid userId);

        /// <summary>
        /// Deletes a notification for the current user.
        /// </summary>
        Task DeleteNotificationAsync(Guid userId, Guid notificationId);

        /// <summary>
        /// Gets or creates a notification preference for a user.
        /// </summary>
        Task<NotificationPreferenceDto> UpdatePreferenceAsync(Guid userId, NotificationType type, bool isEnabled);

        /// <summary>
        /// Gets all notification preferences for a user.
        /// </summary>
        Task<IEnumerable<NotificationPreferenceDto>> GetPreferencesAsync(Guid userId);
    }
}
