using MediatR;
using TestFirstProject.DTOs;

namespace TestFirstProject.Events
{
    /// <summary>
    /// Domain event published when a new message is sent.
    /// Triggers notification creation and real-time delivery.
    /// </summary>
    public record MessageReceivedEvent(
        MessageDto Message,
        Guid ConversationId,
        Guid RecipientId,
        string SenderUsername
    ) : INotification;
}
