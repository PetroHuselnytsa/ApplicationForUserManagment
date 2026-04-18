namespace TestFirstProject.Settings
{
    /// <summary>
    /// Strongly-typed configuration for JWT token generation.
    /// Bound from the "JwtSettings" section in appsettings.json.
    /// The SecretKey should be overridden by environment variable JWT_SECRET_KEY in production.
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string SecretKey { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;

        /// <summary>Access token lifetime in minutes.</summary>
        public int AccessTokenExpirationMinutes { get; set; } = 15;

        /// <summary>Refresh token lifetime in days.</summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}
