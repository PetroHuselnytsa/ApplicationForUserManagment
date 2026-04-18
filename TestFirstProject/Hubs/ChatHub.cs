using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ISignalRNotifier _signalRNotifier;

        public ChatHub(ISignalRNotifier signalRNotifier)
        {
            _signalRNotifier = signalRNotifier;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendTypingIndicator(Guid conversationId, Guid recipientId)
        {
            var userId = GetUserId();
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            if (userId != null)
            {
                await _signalRNotifier.SendTypingIndicatorAsync(recipientId, conversationId, username);
            }
        }

        private string? GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
