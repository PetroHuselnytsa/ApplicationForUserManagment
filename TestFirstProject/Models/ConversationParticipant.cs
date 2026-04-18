namespace TestFirstProject.Models
{
    /// <summary>
    /// Join entity linking a User to a Conversation.
    /// Enables many-to-many between users and conversations.
    /// </summary>
    public class ConversationParticipant
    {
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
