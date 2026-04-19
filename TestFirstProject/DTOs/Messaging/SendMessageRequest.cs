using System.ComponentModel.DataAnnotations;

namespace TestFirstProject.DTOs.Messaging
{
    public class SendMessageRequest
    {
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; } = null!;
    }
}
