namespace TestFirstProject.Models
{
    /// <summary>
    /// Per-user notification preferences. Users can opt out of specific notification types.
    /// </summary>
    public class NotificationPreference
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;

        /// <summary>Whether to receive notifications for new messages.</summary>
        public bool NewMessageEnabled { get; set; } = true;

        /// <summary>Whether to receive notifications for role changes.</summary>
        public bool RoleChangedEnabled { get; set; } = true;

        /// <summary>Whether to receive system alert notifications.</summary>
        public bool SystemAlertEnabled { get; set; } = true;

        /// <summary>Whether to receive user mention notifications.</summary>
        public bool UserMentionedEnabled { get; set; } = true;
    }
}
