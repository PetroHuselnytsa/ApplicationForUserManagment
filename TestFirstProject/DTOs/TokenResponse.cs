namespace TestFirstProject.DTOs
{
    /// <summary>
    /// Response DTO containing access and refresh tokens.
    /// </summary>
    public class TokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime AccessTokenExpiresAt { get; set; }
    }
}
