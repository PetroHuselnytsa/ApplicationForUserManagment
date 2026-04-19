using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestFirstProject.DTOs.Common;
using TestFirstProject.DTOs.Messaging;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Controllers
{
    /// <summary>
    /// Manages conversations and messages between authenticated users.
    /// All endpoints require JWT authentication.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ISignalRNotifier _signalRNotifier;
        private readonly INotificationQueue _notificationQueue;
        private readonly IMessageRateLimiter _rateLimiter;

        public ConversationsController(
            IMessageService messageService,
            ISignalRNotifier signalRNotifier,
            INotificationQueue notificationQueue,
            IMessageRateLimiter rateLimiter)
        {
            _messageService = messageService;
            _signalRNotifier = signalRNotifier;
            _notificationQueue = notificationQueue;
            _rateLimiter = rateLimiter;
        }

        /// <summary>
        /// Start a new conversation or get an existing one with a specific user.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ConversationDto>> StartOrGetConversation(
            [FromBody] StartConversationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();
            var conversation = await _messageService.StartOrGetConversationAsync(currentUserId, request.RecipientId);

            return Ok(conversation);
        }

        /// <summary>
        /// List all conversations for the current user with last message preview and unread count.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<ConversationDto>>> GetConversations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _messageService.GetConversationsAsync(currentUserId, page, pageSize);

            return Ok(result);
        }

        /// <summary>
        /// Get paginated message history for a conversation.
        /// Supports cursor-based pagination — pass the cursor from the previous response to get the next page.
        /// </summary>
        [HttpGet("{id}/messages")]
        public async Task<ActionResult<CursorPagedResult<MessageDto>>> GetMessages(
            Guid id,
            [FromQuery] string? cursor = null,
            [FromQuery] int pageSize = 50)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _messageService.GetMessagesAsync(currentUserId, id, cursor, pageSize);

            return Ok(result);
        }

        /// <summary>
        /// Send a message in a conversation.
        /// Rate limited: max 30 messages per minute per user.
        /// </summary>
        [HttpPost("{id}/messages")]
        public async Task<ActionResult<MessageDto>> SendMessage(
            Guid id,
            [FromBody] SendMessageRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();

            // Enforce rate limit
            _rateLimiter.ValidateAndRecord(currentUserId);

            var message = await _messageService.SendMessageAsync(currentUserId, id, request.Content);

            // Push the message to the recipient via SignalR in real time
            var recipientId = await _messageService.GetConversationRecipientAsync(id, currentUserId);
            await _signalRNotifier.SendMessageToUserAsync(recipientId, message, id);

            // Enqueue a background notification for the recipient
            await _notificationQueue.EnqueueAsync(new NotificationRequest
            {
                UserId = recipientId,
                Type = NotificationType.NewMessage,
                Title = "New Message",
                Body = $"{message.SenderUsername}: {Truncate(message.Content, 100)}"
            });

            return Created($"api/conversations/{id}/messages/{message.Id}", message);
        }

        /// <summary>
        /// Mark a specific message as read.
        /// </summary>
        [HttpPut("{id}/messages/{messageId}/read")]
        public async Task<IActionResult> MarkMessageAsRead(Guid id, Guid messageId)
        {
            var currentUserId = GetCurrentUserId();
            await _messageService.MarkMessageAsReadAsync(currentUserId, id, messageId);

            // Notify the sender that their message was read
            try
            {
                var senderId = await _messageService.GetConversationRecipientAsync(id, currentUserId);
                await _signalRNotifier.SendMessageReadReceiptAsync(senderId, id, messageId);
            }
            catch
            {
                // Non-critical: if we can't send the read receipt, that's ok
            }

            return NoContent();
        }

        /// <summary>
        /// Soft-delete a message (sender can delete from their side only).
        /// </summary>
        [HttpDelete("{id}/messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid id, Guid messageId)
        {
            var currentUserId = GetCurrentUserId();
            await _messageService.SoftDeleteMessageAsync(currentUserId, id, messageId);

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

        /// <summary>
        /// Truncate a string to a maximum length, appending "..." if truncated.
        /// </summary>
        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value[..maxLength] + "...";
        }
    }
}
