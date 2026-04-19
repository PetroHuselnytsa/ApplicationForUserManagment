using System.ComponentModel.DataAnnotations;

namespace TestFirstProject.DTOs.Messaging
{
    public class StartConversationRequest
    {
        [Required]
        public Guid RecipientId { get; set; }
    }
}
