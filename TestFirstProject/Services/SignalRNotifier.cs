using Microsoft.AspNetCore.SignalR;
using TestFirstProject.DTOs.Messaging;
using TestFirstProject.DTOs.Notifications;
using TestFirstProject.Hubs;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Pushes real-time events to connected SignalR clients.
    /// Each user connects to a personal group identified by their user ID.
    /// </summary>
    public class SignalRNotifier : ISignalRNotifier
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<SignalRNotifier> _logger;

        public SignalRNotifier(IHubContext<ChatHub> hubContext, ILogger<SignalRNotifier> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendMessageToUserAsync(Guid userId, MessageDto message, Guid conversationId)
        {
            try
            {
                await _hubContext.Clients.Group(userId.ToString())
                    .SendAsync("ReceiveMessage", new
                    {
                        conversationId,
                        message
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send real-time message to user {UserId}", userId);
            }
        }

        public async Task SendNotificationToUserAsync(Guid userId, NotificationDto notification)
        {
            try
            {
                await _hubContext.Clients.Group(userId.ToString())
                    .SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send real-time notification to user {UserId}", userId);
            }
        }

        public async Task SendTypingIndicatorAsync(
            Guid userId, Guid conversationId, Guid typingUserId, string username)
        {
            try
            {
                await _hubContext.Clients.Group(userId.ToString())
                    .SendAsync("UserTyping", new
                    {
                        conversationId,
                        userId = typingUserId,
                        username
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send typing indicator to user {UserId}", userId);
            }
        }

        public async Task SendMessageReadReceiptAsync(Guid userId, Guid conversationId, Guid messageId)
        {
            try
            {
                await _hubContext.Clients.Group(userId.ToString())
                    .SendAsync("MessageRead", new
                    {
                        conversationId,
                        messageId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send read receipt to user {UserId}", userId);
            }
        }
    }
}
