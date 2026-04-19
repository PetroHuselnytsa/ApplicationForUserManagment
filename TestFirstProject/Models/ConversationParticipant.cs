namespace TestFirstProject.Models
{
    /// <summary>
    /// Join entity linking a User to a Conversation.
    /// Tracks per-user metadata such as the last read timestamp.
    /// </summary>
    public class ConversationParticipant
    {
        public Guid ConversationId { get; set; }
        public Guid UserId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Conversation Conversation { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
