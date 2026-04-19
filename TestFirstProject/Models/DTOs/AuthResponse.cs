namespace TestFirstProject.Models.DTOs
{
    /// <summary>
    /// Response returned on successful login or token refresh.
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Short-lived JWT access token for API authorization.
        /// </summary>
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// Long-lived opaque refresh token for obtaining new access tokens.
        /// </summary>
        public string RefreshToken { get; set; } = null!;

        /// <summary>
        /// UTC expiration time of the access token.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
