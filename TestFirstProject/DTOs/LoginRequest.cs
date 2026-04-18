using System.ComponentModel.DataAnnotations;

namespace TestFirstProject.DTOs
{
    /// <summary>
    /// Request DTO for user login.
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;
    }
}
