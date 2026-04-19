using TestFirstProject.DTOs.Common;
using TestFirstProject.DTOs.Notifications;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Manages notifications for users, including creation, retrieval, and status updates.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Create a notification for a specific user.
        /// Respects user notification preferences.
        /// </summary>
        Task<NotificationDto?> CreateNotificationAsync(Guid userId, NotificationType type, string title, string body);

        /// <summary>
        /// Get paginated notifications for a user.
        /// </summary>
        Task<PagedResult<NotificationDto>> GetNotificationsAsync(Guid userId, int page, int pageSize);

        /// <summary>
        /// Mark a single notification as read.
        /// </summary>
        Task MarkAsReadAsync(Guid userId, Guid notificationId);

        /// <summary>
        /// Mark all notifications for a user as read.
        /// </summary>
        Task MarkAllAsReadAsync(Guid userId);

        /// <summary>
        /// Delete a notification.
        /// </summary>
        Task DeleteNotificationAsync(Guid userId, Guid notificationId);

        /// <summary>
        /// Update a user's notification preference for a specific type.
        /// </summary>
        Task UpdatePreferenceAsync(Guid userId, NotificationType type, bool isEnabled);

        /// <summary>
        /// Get all notification preferences for a user.
        /// </summary>
        Task<List<UpdateNotificationPreferenceRequest>> GetPreferencesAsync(Guid userId);
    }
}
