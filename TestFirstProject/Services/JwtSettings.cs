using Microsoft.IdentityModel.Tokens;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Immutable JWT configuration consumed by TokenService and Program.cs.
    /// Constructed once at startup to avoid re-reading config and re-creating keys per request.
    /// </summary>
    public sealed record JwtSettings(
        string Secret,
        string Issuer,
        string Audience,
        SymmetricSecurityKey SigningKey,
        int AccessTokenExpirationMinutes
    );
}
