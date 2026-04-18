namespace TestFirstProject.Models
{
    /// <summary>
    /// Join entity for the many-to-many relationship between AppUser and Conversation.
    /// </summary>
    public class ConversationParticipant
    {
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
