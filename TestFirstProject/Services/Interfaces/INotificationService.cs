using TestFirstProject.DTOs;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Handles notification CRUD, read status, and creation triggered by domain events.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>Get paginated notifications for a user.</summary>
        Task<PagedResult<NotificationDto>> GetNotificationsAsync(Guid userId, int page = 1, int pageSize = 20);

        /// <summary>Create a notification for a user (called from domain event handlers). Returns null if user opted out.</summary>
        Task<NotificationDto?> CreateNotificationAsync(Guid userId, NotificationType type, string title, string body);

        /// <summary>Mark a single notification as read.</summary>
        Task MarkAsReadAsync(Guid userId, Guid notificationId);

        /// <summary>Mark all notifications as read for a user.</summary>
        Task MarkAllAsReadAsync(Guid userId);

        /// <summary>Delete a notification.</summary>
        Task DeleteNotificationAsync(Guid userId, Guid notificationId);

        /// <summary>Update notification preferences for a user.</summary>
        Task UpdatePreferencesAsync(Guid userId, UpdateNotificationPreferenceRequest request);

        /// <summary>Get notification preferences for a user.</summary>
        Task<UpdateNotificationPreferenceRequest> GetPreferencesAsync(Guid userId);
    }
}
