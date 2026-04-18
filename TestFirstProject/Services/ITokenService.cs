using TestFirstProject.Models;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Service responsible for generating and validating JWT access tokens and refresh tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token for the given user, including their roles as claims.
        /// </summary>
        string GenerateAccessToken(User user, IEnumerable<string> roles);

        /// <summary>
        /// Generates a cryptographically secure random refresh token string.
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Returns the expiration time for a new access token.
        /// </summary>
        DateTime GetAccessTokenExpiration();
    }
}
