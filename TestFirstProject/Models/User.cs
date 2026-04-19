using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents an application user with authentication credentials and role assignment.
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public UserRole Role { get; set; } = UserRole.User;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<NotificationPreference> NotificationPreferences { get; set; } = new List<NotificationPreference>();
    }
}
