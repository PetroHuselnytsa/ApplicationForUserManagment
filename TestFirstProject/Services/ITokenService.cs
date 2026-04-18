using TestFirstProject.Models;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Interface for JWT and refresh token generation/validation.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token containing the user's claims (email, roles).
        /// </summary>
        string GenerateAccessToken(User user, IEnumerable<string> roles);

        /// <summary>
        /// Generates a cryptographically secure refresh token string.
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Returns the configured access token expiration time.
        /// </summary>
        DateTime GetAccessTokenExpiration();
    }
}
