using Microsoft.AspNetCore.SignalR;
using TestFirstProject.DTOs;
using TestFirstProject.Hubs;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class SignalRNotifier : ISignalRNotifier
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public SignalRNotifier(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendMessageToUserAsync(Guid userId, MessageDto message, Guid conversationId)
        {
            await UserGroup(userId)
                .SendAsync("ReceiveMessage", new { conversationId, message });
        }

        public async Task SendNotificationToUserAsync(Guid userId, NotificationDto notification)
        {
            await UserGroup(userId)
                .SendAsync("ReceiveNotification", notification);
        }

        public async Task SendTypingIndicatorAsync(Guid userId, Guid conversationId, string senderUsername)
        {
            await UserGroup(userId)
                .SendAsync("UserTyping", new { conversationId, username = senderUsername });
        }

        private IClientProxy UserGroup(Guid userId) =>
            _hubContext.Clients.Group(userId.ToString());
    }
}
