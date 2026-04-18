namespace TestFirstProject.Settings
{
    /// <summary>
    /// Configuration class for JWT token settings.
    /// Values should be provided via appsettings.json or environment variables.
    /// Environment variable overrides use the pattern: JwtSettings__PropertyName
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        /// <summary>
        /// The secret key used to sign JWT tokens.
        /// Override via environment variable: JwtSettings__SecretKey
        /// MUST be at least 32 characters for HMAC-SHA256.
        /// </summary>
        public string SecretKey { get; set; } = null!;

        /// <summary>
        /// The issuer claim for generated tokens.
        /// Override via environment variable: JwtSettings__Issuer
        /// </summary>
        public string Issuer { get; set; } = null!;

        /// <summary>
        /// The audience claim for generated tokens.
        /// Override via environment variable: JwtSettings__Audience
        /// </summary>
        public string Audience { get; set; } = null!;

        /// <summary>
        /// Access token expiration time in minutes.
        /// Override via environment variable: JwtSettings__AccessTokenExpirationMinutes
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; } = 15;

        /// <summary>
        /// Refresh token expiration time in days.
        /// Override via environment variable: JwtSettings__RefreshTokenExpirationDays
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}
