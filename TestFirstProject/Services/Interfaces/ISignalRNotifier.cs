using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Abstraction for pushing real-time events to connected clients via SignalR.
    /// Services call this instead of directly referencing the Hub.
    /// </summary>
    public interface ISignalRNotifier
    {
        /// <summary>Push a new message to the recipient if they are online.</summary>
        Task SendMessageToUserAsync(Guid userId, MessageDto message, Guid conversationId);

        /// <summary>Push a notification to the target user if they are online.</summary>
        Task SendNotificationToUserAsync(Guid userId, NotificationDto notification);

        /// <summary>Push a typing indicator to the other participant in a conversation.</summary>
        Task SendTypingIndicatorAsync(Guid userId, Guid conversationId, string senderUsername);
    }
}
