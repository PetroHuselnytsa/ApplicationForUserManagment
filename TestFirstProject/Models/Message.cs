namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a message within a conversation.
    /// Supports soft delete per-side (sender can delete from their view only).
    /// </summary>
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public Guid SenderId { get; set; }
        public AppUser Sender { get; set; } = null!;

        public string Content { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        // Soft delete flags — each side can independently delete a message from their view
        public bool IsDeletedBySender { get; set; } = false;
        public bool IsDeletedByRecipient { get; set; } = false;
    }
}
