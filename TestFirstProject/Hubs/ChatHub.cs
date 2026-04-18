using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Hubs
{
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

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.GetUserGuid();
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId.Value.ToString());
                _logger.LogInformation("User {UserId} connected to ChatHub (ConnectionId: {ConnectionId})",
                    userId, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User.GetUserGuid();
            if (userId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.Value.ToString());
                _logger.LogInformation("User {UserId} disconnected from ChatHub (ConnectionId: {ConnectionId})",
                    userId, Context.ConnectionId);
            }

            if (exception != null)
            {
                _logger.LogWarning(exception, "User {UserId} disconnected with error", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendTypingIndicator(Guid conversationId)
        {
            var userId = Context.User.GetUserGuid();
            if (userId == null) return;

            var userName = Context.User?.FindFirstValue("name") ?? "Unknown";

            try
            {
                var recipientId = await _messageService.GetRecipientUserIdAsync(userId.Value, conversationId);

                await Clients.Group(recipientId.ToString())
                    .SendAsync("UserTyping", new
                    {
                        ConversationId = conversationId,
                        UserId = userId.Value,
                        UserName = userName
                    });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send typing indicator for conversation {ConversationId}", conversationId);
            }
        }
    }
}
