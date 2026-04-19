using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a notification sent to a specific user.
    /// Notifications are server-generated on domain events.
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

        // Navigation property
        public User User { get; set; } = null!;
    }
}
