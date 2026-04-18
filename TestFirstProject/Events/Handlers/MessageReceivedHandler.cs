using MediatR;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Events.Handlers
{
    /// <summary>
    /// Handles the MessageReceivedEvent:
    /// 1) Creates a notification for the recipient
    /// 2) Pushes the message + notification in real time via SignalR
    /// </summary>
    public class MessageReceivedHandler : INotificationHandler<MessageReceivedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ISignalRNotifier _signalRNotifier;

        public MessageReceivedHandler(
            INotificationService notificationService,
            ISignalRNotifier signalRNotifier)
        {
            _notificationService = notificationService;
            _signalRNotifier = signalRNotifier;
        }

        public async Task Handle(MessageReceivedEvent @event, CancellationToken cancellationToken)
        {
            // 1. Push message to recipient in real time
            await _signalRNotifier.SendMessageToUserAsync(
                @event.RecipientId,
                @event.Message,
                @event.ConversationId);

            // 2. Create a persistent notification
            var notification = await _notificationService.CreateNotificationAsync(
                @event.RecipientId,
                NotificationType.NewMessage,
                $"New message from {@event.SenderUsername}",
                @event.Message.Content.Length > 100
                    ? @event.Message.Content[..100] + "..."
                    : @event.Message.Content);

            // 3. Push notification in real time (if it was actually created — not suppressed by preferences)
            if (notification != null)
            {
                await _signalRNotifier.SendNotificationToUserAsync(@event.RecipientId, notification);
            }
        }
    }
}
