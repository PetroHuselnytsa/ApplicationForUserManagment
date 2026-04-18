namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a message within a conversation.
    /// Supports soft-delete per side (sender/recipient can independently delete their view).
    /// </summary>
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        // Soft-delete flags — each side can independently hide the message
        public bool IsDeletedBySender { get; set; } = false;
        public bool IsDeletedByRecipient { get; set; } = false;

        // Navigation properties
        public Conversation Conversation { get; set; } = null!;
        public User Sender { get; set; } = null!;
    }
}
