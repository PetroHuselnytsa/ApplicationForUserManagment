using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestFirstProject.DTOs.Common;
using TestFirstProject.DTOs.Notifications;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Controllers
{
    /// <summary>
    /// Manages notifications for the authenticated user.
    /// All endpoints require JWT authentication.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get paginated list of notifications for the current user.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<NotificationDto>>> GetNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.GetNotificationsAsync(userId, page, pageSize);

            return Ok(result);
        }

        /// <summary>
        /// Mark a single notification as read.
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAsReadAsync(userId, id);

            return NoContent();
        }

        /// <summary>
        /// Mark all notifications as read for the current user.
        /// </summary>
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);

            return NoContent();
        }

        /// <summary>
        /// Delete a notification.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var userId = GetCurrentUserId();
            await _notificationService.DeleteNotificationAsync(userId, id);

            return NoContent();
        }

        /// <summary>
        /// Get notification preferences for the current user.
        /// </summary>
        [HttpGet("preferences")]
        public async Task<ActionResult<List<UpdateNotificationPreferenceRequest>>> GetPreferences()
        {
            var userId = GetCurrentUserId();
            var preferences = await _notificationService.GetPreferencesAsync(userId);

            return Ok(preferences);
        }

        /// <summary>
        /// Update a notification preference for the current user.
        /// </summary>
        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreference([FromBody] UpdateNotificationPreferenceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            await _notificationService.UpdatePreferenceAsync(userId, request.NotificationType, request.IsEnabled);

            return NoContent();
        }

        /// <summary>
        /// Extract current user ID from JWT claims.
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId)
                ? userId
                : throw new UnauthorizedAccessException("Invalid user token.");
        }
    }
}
