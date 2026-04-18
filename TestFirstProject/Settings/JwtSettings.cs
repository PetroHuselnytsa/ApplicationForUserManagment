namespace TestFirstProject.Settings
{
    /// <summary>
    /// Configuration for JWT token generation and validation.
    /// Bound from appsettings.json "JwtSettings" section.
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string Secret { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int ExpirationInMinutes { get; set; } = 60;
    }
}
