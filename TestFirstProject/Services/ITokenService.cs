using TestFirstProject.Models;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Service responsible for generating and validating JWT access tokens and refresh tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a short-lived JWT access token containing user claims and roles.
        /// </summary>
        /// <param name="user">The user for whom the token is generated.</param>
        /// <param name="roles">The user's role names to embed as claims.</param>
        /// <returns>A tuple of (accessToken, expiresAt).</returns>
        (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, IEnumerable<string> roles);

        /// <summary>
        /// Generates a cryptographically secure opaque refresh token.
        /// </summary>
        /// <returns>The refresh token string.</returns>
        string GenerateRefreshToken();
    }
}
