using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly PersonsContext _context;

        public NotificationService(PersonsContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            int totalCount = await query.CountAsync();

            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto(
                    n.Id,
                    n.Type.ToString(),
                    n.Title,
                    n.Body,
                    n.IsRead,
                    n.CreatedAt,
                    n.ReadAt
                ))
                .ToListAsync();

            return new PagedResult<NotificationDto>(notifications, totalCount, page, pageSize);
        }

        public async Task<NotificationDto?> CreateNotificationAsync(
            Guid userId, NotificationType type, string title, string body)
        {
            if (!await IsNotificationTypeEnabledAsync(userId, type))
                return null;

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Title = title,
                Body = body,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return MapToDto(notification);
        }

        public async Task MarkAsReadAsync(Guid userId, Guid notificationId)
        {
            var notification = await GetNotificationOrThrowAsync(userId, notificationId);

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var now = DateTime.UtcNow;

            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, now));
        }

        public async Task DeleteNotificationAsync(Guid userId, Guid notificationId)
        {
            var notification = await GetNotificationOrThrowAsync(userId, notificationId);

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsNotificationTypeEnabledAsync(Guid userId, NotificationType type)
        {
            var pref = await GetPreferenceEntityAsync(userId);

            if (pref == null)
                return true;

            return type switch
            {
                NotificationType.NewMessage => pref.NewMessageEnabled,
                NotificationType.RoleChanged => pref.RoleChangedEnabled,
                NotificationType.SystemAlert => pref.SystemAlertEnabled,
                NotificationType.UserMentioned => pref.UserMentionedEnabled,
                _ => true
            };
        }

        public async Task UpdatePreferencesAsync(Guid userId, UpdateNotificationPreferenceRequest request)
        {
            var pref = await GetPreferenceEntityAsync(userId);

            if (pref == null)
            {
                pref = new NotificationPreference
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                _context.NotificationPreferences.Add(pref);
            }

            pref.NewMessageEnabled = request.NewMessageEnabled;
            pref.RoleChangedEnabled = request.RoleChangedEnabled;
            pref.SystemAlertEnabled = request.SystemAlertEnabled;
            pref.UserMentionedEnabled = request.UserMentionedEnabled;

            await _context.SaveChangesAsync();
        }

        public async Task<UpdateNotificationPreferenceRequest> GetPreferencesAsync(Guid userId)
        {
            var pref = await GetPreferenceEntityAsync(userId);

            if (pref == null)
                return new UpdateNotificationPreferenceRequest(true, true, true, true);

            return new UpdateNotificationPreferenceRequest(
                pref.NewMessageEnabled,
                pref.RoleChangedEnabled,
                pref.SystemAlertEnabled,
                pref.UserMentionedEnabled
            );
        }

        // --- Private helpers ---

        private async Task<Notification> GetNotificationOrThrowAsync(Guid userId, Guid notificationId)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId)
                ?? throw new NotFoundException("Notification not found.");
        }

        private async Task<NotificationPreference?> GetPreferenceEntityAsync(Guid userId)
        {
            return await _context.NotificationPreferences
                .FirstOrDefaultAsync(np => np.UserId == userId);
        }

        private static NotificationDto MapToDto(Notification n)
        {
            return new NotificationDto(
                n.Id,
                n.Type.ToString(),
                n.Title,
                n.Body,
                n.IsRead,
                n.CreatedAt,
                n.ReadAt
            );
        }
    }
}
