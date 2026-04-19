using TestFirstProject.Models.Enums;

namespace TestFirstProject.DTOs.Notifications
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
