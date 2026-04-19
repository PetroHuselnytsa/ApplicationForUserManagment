namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a conversation between two users.
    /// Uses a join table (ConversationParticipant) to link users to conversations.
    /// </summary>
    public class Conversation
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastMessageAt { get; set; }

        // Navigation properties
        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
