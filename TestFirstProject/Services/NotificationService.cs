using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Common;
using TestFirstProject.DTOs.Notifications;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Manages notification creation, retrieval, status updates, and user preferences.
    /// Respects per-user notification preferences when creating notifications.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly PersonsContext _context;

        public NotificationService(PersonsContext context)
        {
            _context = context;
        }

        public async Task<NotificationDto?> CreateNotificationAsync(
            Guid userId, NotificationType type, string title, string body)
        {
            // Check if the user has disabled this notification type
            var preference = await _context.NotificationPreferences
                .FirstOrDefaultAsync(np => np.UserId == userId && np.NotificationType == type);

            if (preference != null && !preference.IsEnabled)
                return null; // User has opted out of this notification type

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Title = title,
                Body = body,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return MapToDto(notification);
        }

        public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(Guid userId, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();

            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<NotificationDto>
            {
                Items = notifications.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task MarkAsReadAsync(Guid userId, Guid notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId)
                ?? throw new NotFoundException("Notification", notificationId);

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task DeleteNotificationAsync(Guid userId, Guid notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId)
                ?? throw new NotFoundException("Notification", notificationId);

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePreferenceAsync(Guid userId, NotificationType type, bool isEnabled)
        {
            var preference = await _context.NotificationPreferences
                .FirstOrDefaultAsync(np => np.UserId == userId && np.NotificationType == type);

            if (preference == null)
            {
                preference = new NotificationPreference
                {
                    Id = Guid.NewGuid(),
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
        }

        public async Task<List<UpdateNotificationPreferenceRequest>> GetPreferencesAsync(Guid userId)
        {
            var preferences = await _context.NotificationPreferences
                .Where(np => np.UserId == userId)
                .ToListAsync();

            return preferences.Select(p => new UpdateNotificationPreferenceRequest
            {
                NotificationType = p.NotificationType,
                IsEnabled = p.IsEnabled
            }).ToList();
        }

        private static NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Title = notification.Title,
                Body = notification.Body,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}
