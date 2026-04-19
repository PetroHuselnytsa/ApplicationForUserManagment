using System.ComponentModel.DataAnnotations;

namespace TestFirstProject.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(128, MinimumLength = 6)]
        public string Password { get; set; } = null!;
    }
}
