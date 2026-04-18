using Microsoft.AspNetCore.SignalR;
using TestFirstProject.DTOs;
using TestFirstProject.Hubs;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services
{
    public class SignalRNotifier : ISignalRNotifier
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<SignalRNotifier> _logger;

        public SignalRNotifier(IHubContext<ChatHub> hubContext, ILogger<SignalRNotifier> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <inheritdoc />
        public Task NotifyNewMessageAsync(Guid recipientUserId, MessageDto message)
            => SendToGroupAsync(recipientUserId, "ReceiveMessage", message, "message");

        /// <inheritdoc />
        public Task NotifyNewNotificationAsync(Guid userId, NotificationDto notification)
            => SendToGroupAsync(userId, "ReceiveNotification", notification, "notification");

        /// <inheritdoc />
        public Task NotifyUserTypingAsync(
            Guid recipientUserId, Guid conversationId, Guid senderUserId, string senderName)
            => SendToGroupAsync(recipientUserId, "UserTyping", new
            {
                ConversationId = conversationId,
                UserId = senderUserId,
                UserName = senderName
            }, "typing indicator");

        /// <inheritdoc />
        public Task NotifyMessageReadAsync(Guid senderUserId, Guid conversationId, Guid messageId)
            => SendToGroupAsync(senderUserId, "MessageRead", new
            {
                ConversationId = conversationId,
                MessageId = messageId,
                ReadAt = DateTime.UtcNow
            }, "read receipt");

        private async Task SendToGroupAsync(Guid userId, string method, object payload, string eventKind)
        {
            try
            {
                await _hubContext.Clients
                    .Group(userId.ToString())
                    .SendAsync(method, payload);

                _logger.LogDebug("Real-time {EventKind} pushed to user {UserId}", eventKind, userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to push {EventKind} to user {UserId}", eventKind, userId);
            }
        }
    }
}
