using System.ComponentModel.DataAnnotations;

namespace TestFirstProject.Models.DTOs
{
    /// <summary>
    /// Request payload for refreshing an access token.
    /// </summary>
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = null!;
    }
}
