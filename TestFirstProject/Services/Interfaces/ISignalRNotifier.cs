using TestFirstProject.DTOs.Messaging;
using TestFirstProject.DTOs.Notifications;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Abstraction for pushing real-time events to connected clients via SignalR.
    /// Decouples business logic from SignalR hub implementation details.
    /// </summary>
    public interface ISignalRNotifier
    {
        /// <summary>
        /// Push a new message to a specific user in real time.
        /// </summary>
        Task SendMessageToUserAsync(Guid userId, MessageDto message, Guid conversationId);

        /// <summary>
        /// Push a notification to a specific user in real time.
        /// </summary>
        Task SendNotificationToUserAsync(Guid userId, NotificationDto notification);

        /// <summary>
        /// Push a typing indicator event to a specific user.
        /// </summary>
        Task SendTypingIndicatorAsync(Guid userId, Guid conversationId, Guid typingUserId, string username);

        /// <summary>
        /// Notify a user that a message has been read.
        /// </summary>
        Task SendMessageReadReceiptAsync(Guid userId, Guid conversationId, Guid messageId);
    }
}
