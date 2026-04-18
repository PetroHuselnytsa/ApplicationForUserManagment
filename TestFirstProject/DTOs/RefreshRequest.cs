using System.ComponentModel.DataAnnotations;

namespace TestFirstProject.DTOs
{
    /// <summary>
    /// Request DTO for refreshing an access token.
    /// </summary>
    public class RefreshRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = null!;
    }
}
