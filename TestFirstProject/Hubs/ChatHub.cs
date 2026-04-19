using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Hubs
{
    /// <summary>
    /// SignalR hub for real-time messaging and notifications.
    /// Authenticated connections only — JWT is validated on connect.
    /// Each user is added to a personal group (their user ID) for targeted delivery.
    ///
    /// Client events:
    ///   - ReceiveMessage(data)       — new message pushed to recipient
    ///   - ReceiveNotification(data)  — notification pushed to target user
    ///   - UserTyping(data)           — typing indicator event
    ///   - MessageRead(data)          — read receipt event
    ///
    /// Hub methods (callable from client):
    ///   - SendTypingIndicator(conversationId) — notify the other participant you're typing
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IMessageService messageService, ILogger<ChatHub> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        /// <summary>
        /// Called when a client connects. Adds user to their personal group for targeted messages.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            if (userId != Guid.Empty)
            {
                // Each user gets a personal group named after their user ID
                await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
                _logger.LogInformation("User {UserId} connected to ChatHub (ConnectionId: {ConnectionId})",
                    userId, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects. Removes user from their personal group.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            if (userId != Guid.Empty)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
                _logger.LogInformation("User {UserId} disconnected from ChatHub (ConnectionId: {ConnectionId})",
                    userId, Context.ConnectionId);
            }

            if (exception != null)
            {
                _logger.LogWarning(exception, "User {UserId} disconnected with error", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Client invokes this to broadcast a typing indicator to the other participant.
        /// Hub delegates to service layer — no business logic here.
        /// </summary>
        public async Task SendTypingIndicator(Guid conversationId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return;

            try
            {
                var recipientId = await _messageService.GetConversationRecipientAsync(conversationId, userId);
                var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

                // Send typing indicator to the other user's personal group
                await Clients.Group(recipientId.ToString())
                    .SendAsync("UserTyping", new
                    {
                        conversationId,
                        userId,
                        username
                    });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send typing indicator for conversation {ConversationId}", conversationId);
            }
        }

        /// <summary>
        /// Extract the current user's ID from JWT claims.
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
