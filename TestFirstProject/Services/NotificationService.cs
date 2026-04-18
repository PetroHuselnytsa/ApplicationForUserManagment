using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Implementation of INotificationService handling notification CRUD,
    /// read status, and user notification preferences.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly PersonsContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(PersonsContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<NotificationDto?> CreateNotificationAsync(
            Guid userId,
            NotificationType type,
            string title,
            string body,
            string? referenceId = null)
        {
            // Check user notification preference — if disabled, skip
            var preference = await _context.NotificationPreferences
                .FirstOrDefaultAsync(np => np.UserId == userId && np.NotificationType == type);

            if (preference != null && !preference.IsEnabled)
            {
                _logger.LogDebug("Notification of type {Type} skipped for user {UserId} — disabled by preference",
                    type, userId);
                return null;
            }

            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Body = body,
                ReferenceId = referenceId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification {NotificationId} of type {Type} created for user {UserId}",
                notification.Id, type, userId);

            return MapToDto(notification);
        }

        /// <inheritdoc />
        public async Task<PaginatedNotificationsDto> GetNotificationsAsync(Guid userId, int page, int pageSize)
        {
            // Clamp parameters
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedNotificationsDto(
                Notifications: notifications.Select(MapToDto),
                Page: page,
                PageSize: pageSize,
                TotalCount: totalCount,
                TotalPages: totalPages
            );
        }

        /// <inheritdoc />
        public async Task MarkAsReadAsync(Guid userId, Guid notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                throw new NotFoundException("Notification not found.");
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var now = DateTime.UtcNow;

            var updatedCount = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, now));

            if (updatedCount > 0)
            {
                _logger.LogInformation("Marked {Count} notifications as read for user {UserId}",
                    updatedCount, userId);
            }
        }

        /// <inheritdoc />
        public async Task DeleteNotificationAsync(Guid userId, Guid notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                throw new NotFoundException("Notification not found.");
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification {NotificationId} deleted by user {UserId}",
                notificationId, userId);
        }

        /// <inheritdoc />
        public async Task<NotificationPreferenceDto> UpdatePreferenceAsync(
            Guid userId, NotificationType type, bool isEnabled)
        {
            var preference = await _context.NotificationPreferences
                .FirstOrDefaultAsync(np => np.UserId == userId && np.NotificationType == type);

            if (preference == null)
            {
                // Create new preference
                preference = new NotificationPreference
                {
                    UserId = userId,
                    NotificationType = type,
                    IsEnabled = isEnabled
                };
                _context.NotificationPreferences.Add(preference);
            }
            else
            {
                preference.IsEnabled = isEnabled;
            }

            await _context.SaveChangesAsync();

            return new NotificationPreferenceDto(
                Id: preference.Id,
                NotificationType: type.ToString(),
                IsEnabled: preference.IsEnabled
            );
        }

        /// <inheritdoc />
        public async Task<IEnumerable<NotificationPreferenceDto>> GetPreferencesAsync(Guid userId)
        {
            var preferences = await _context.NotificationPreferences
                .Where(np => np.UserId == userId)
                .ToListAsync();

            return preferences.Select(p => new NotificationPreferenceDto(
                Id: p.Id,
                NotificationType: p.NotificationType.ToString(),
                IsEnabled: p.IsEnabled
            ));
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        private static NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto(
                Id: notification.Id,
                Type: notification.Type.ToString(),
                Title: notification.Title,
                Body: notification.Body,
                IsRead: notification.IsRead,
                CreatedAt: notification.CreatedAt,
                ReadAt: notification.ReadAt,
                ReferenceId: notification.ReferenceId
            );
        }
    }
}
