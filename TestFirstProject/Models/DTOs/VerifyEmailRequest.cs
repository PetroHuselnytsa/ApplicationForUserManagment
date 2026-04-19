using System.ComponentModel.DataAnnotations;

namespace TestFirstProject.Models.DTOs
{
    /// <summary>
    /// Request payload for verifying a user's email address.
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
