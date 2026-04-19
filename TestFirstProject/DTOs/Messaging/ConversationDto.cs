namespace TestFirstProject.DTOs.Messaging
{
    public class ConversationDto
    {
        public Guid Id { get; set; }
        public Guid OtherUserId { get; set; }
        public string OtherUsername { get; set; } = null!;
        public string? LastMessageContent { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
