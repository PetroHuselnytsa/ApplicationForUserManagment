namespace TestFirstProject.Models
{
    /// <summary>
    /// Stores per-user notification preferences.
    /// Users can disable certain notification types.
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
