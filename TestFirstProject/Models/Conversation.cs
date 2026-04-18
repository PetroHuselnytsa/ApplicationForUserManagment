namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a conversation between two users.
    /// A conversation is unique per pair of users.
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
