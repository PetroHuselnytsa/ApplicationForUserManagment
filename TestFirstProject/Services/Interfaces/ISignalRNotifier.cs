using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Abstraction for pushing real-time notifications and messages to connected clients via SignalR.
    /// Hub logic must not contain business logic — delegate to services via this interface.
    /// </summary>
    public interface ISignalRNotifier
    {
        /// <summary>
        /// Pushes a new message to the recipient user in real time if they are online.
        /// </summary>
        Task NotifyNewMessageAsync(Guid recipientUserId, MessageDto message);

        /// <summary>
        /// Pushes a new notification to the target user in real time if they are online.
        /// </summary>
        Task NotifyNewNotificationAsync(Guid userId, NotificationDto notification);

        /// <summary>
        /// Pushes a typing indicator to the recipient user.
        /// </summary>
        Task NotifyUserTypingAsync(Guid recipientUserId, Guid conversationId, Guid senderUserId, string senderName);

        /// <summary>
        /// Pushes a message read receipt to the sender of the message.
        /// </summary>
        Task NotifyMessageReadAsync(Guid senderUserId, Guid conversationId, Guid messageId);
    }
}
