using System.ComponentModel.DataAnnotations;

namespace TestFirstProject.Models.DTOs
{
    /// <summary>
    /// Request payload for logging out (revoking a refresh token).
    /// </summary>
    public class LogoutRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = null!;
    }
}
