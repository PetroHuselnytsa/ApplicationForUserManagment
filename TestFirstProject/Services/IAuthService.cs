using TestFirstProject.DTOs;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Service responsible for all authentication operations including
    /// registration, login, token refresh, logout, and email verification.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with the given credentials and assigns the default "User" role.
        /// Returns an AuthResponse with tokens on success.
        /// </summary>
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Authenticates a user with email and password. Implements account lockout
        /// after consecutive failed login attempts.
        /// Returns an AuthResponse with tokens on success.
        /// </summary>
        Task<AuthResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Issues a new access token and refresh token using a valid, non-expired,
        /// non-revoked refresh token. The old refresh token is revoked to prevent reuse.
        /// </summary>
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

        /// <summary>
        /// Revokes the specified refresh token, effectively logging out the user
        /// on that device/session.
        /// </summary>
        Task LogoutAsync(LogoutRequest request);

        /// <summary>
        /// Verifies a user's email address using the verification token sent during registration.
        /// </summary>
        Task<bool> VerifyEmailAsync(VerifyEmailRequest request);
    }
}
