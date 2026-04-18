using System.ComponentModel.DataAnnotations;

namespace TestFirstProject.DTOs
{
    /// <summary>
    /// Request DTO for email verification.
    /// </summary>
    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Verification token is required.")]
        public string Token { get; set; } = null!;
    }
}
