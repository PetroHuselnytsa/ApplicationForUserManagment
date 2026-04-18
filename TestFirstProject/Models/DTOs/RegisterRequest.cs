using System.ComponentModel.DataAnnotations;

namespace TestFirstProject.Models.DTOs
{
    /// <summary>
    /// Request payload for user registration.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// A valid email address. Must be unique across all users.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Password must be at least 8 characters and contain uppercase, lowercase, digit, and special character.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; } = null!;

        /// <summary>
        /// Must match the Password field exactly.
        /// </summary>
        [Required(ErrorMessage = "Password confirmation is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
