using TestFirstProject.Models.DTOs;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Service responsible for authentication operations: registration, login, token refresh, and logout.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with the given credentials. Assigns the default "User" role.
        /// </summary>
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Authenticates a user and returns access + refresh tokens.
        /// Implements brute-force protection via account lockout.
        /// </summary>
        Task<AuthResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Validates a refresh token and issues a new access + refresh token pair.
        /// The old refresh token is revoked (token rotation).
        /// </summary>
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

        /// <summary>
        /// Revokes the specified refresh token, effectively logging the user out.
        /// </summary>
        Task LogoutAsync(LogoutRequest request);

        /// <summary>
        /// Verifies a user's email address using the verification token.
        /// </summary>
        Task<bool> VerifyEmailAsync(VerifyEmailRequest request);
    }
}
