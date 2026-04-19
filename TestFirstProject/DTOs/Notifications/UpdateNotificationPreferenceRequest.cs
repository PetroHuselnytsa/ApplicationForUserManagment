using System.ComponentModel.DataAnnotations;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.DTOs.Notifications
{
    public class UpdateNotificationPreferenceRequest
    {
        [Required]
        public NotificationType NotificationType { get; set; }

        [Required]
        public bool IsEnabled { get; set; }
    }
}
