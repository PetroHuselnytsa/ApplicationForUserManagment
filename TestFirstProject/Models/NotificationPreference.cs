using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// Per-user preference to enable/disable specific notification types.
    /// </summary>
    public class NotificationPreference
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public NotificationType NotificationType { get; set; }
        public bool IsEnabled { get; set; } = true;

        // Navigation property
        public User User { get; set; } = null!;
    }
}
