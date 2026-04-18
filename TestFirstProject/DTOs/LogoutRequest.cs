using System.ComponentModel.DataAnnotations;

namespace TestFirstProject.DTOs
{
    /// <summary>
    /// Request DTO for logging out (revoking a refresh token).
    /// </summary>
    public class LogoutRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = null!;
    }
}
