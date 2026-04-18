namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a notification delivered to a specific user.
    /// Notifications are created server-side on specific events.
    /// </summary>
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        // Optional reference to a related entity (e.g. conversation or message ID)
        public string? ReferenceId { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }
}
