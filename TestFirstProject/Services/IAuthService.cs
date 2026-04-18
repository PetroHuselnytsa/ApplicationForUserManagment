using TestFirstProject.DTOs;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Interface for authentication operations: register, login, refresh, logout, and email verification.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with the given email and password.
        /// Assigns the default "User" role and generates an email verification token.
        /// </summary>
        Task<(TokenResponse? Token, ErrorResponse? Error)> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Authenticates a user with email and password.
        /// Handles account lockout after repeated failed attempts.
        /// </summary>
        Task<(TokenResponse? Token, ErrorResponse? Error)> LoginAsync(LoginRequest request);

        /// <summary>
        /// Rotates a refresh token: revokes the old one and issues a new pair.
        /// Prevents reuse of revoked tokens by revoking all descendant tokens.
        /// </summary>
        Task<(TokenResponse? Token, ErrorResponse? Error)> RefreshTokenAsync(RefreshRequest request);

        /// <summary>
        /// Revokes a refresh token, effectively logging the user out.
        /// </summary>
        Task<ErrorResponse?> LogoutAsync(LogoutRequest request);

        /// <summary>
        /// Verifies a user's email address using the verification token.
        /// </summary>
        Task<ErrorResponse?> VerifyEmailAsync(VerifyEmailRequest request);
    }
}
