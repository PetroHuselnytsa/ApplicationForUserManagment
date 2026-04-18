using TestFirstProject.Models;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Defines the contract for authentication and authorization operations.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>Register a new user with email, password, and name.</summary>
        Task<(AuthResponse? Response, ErrorResponse? Error)> RegisterAsync(RegisterRequest request);

        /// <summary>Authenticate a user and return access + refresh tokens.</summary>
        Task<(AuthResponse? Response, ErrorResponse? Error)> LoginAsync(LoginRequest request);

        /// <summary>Issue a new access token using a valid refresh token (rotation).</summary>
        Task<(AuthResponse? Response, ErrorResponse? Error)> RefreshAsync(RefreshRequest request);

        /// <summary>Revoke a refresh token (logout).</summary>
        Task<ErrorResponse?> LogoutAsync(LogoutRequest request);

        /// <summary>Mark a user's email as verified using a verification token.</summary>
        Task<(object? Response, ErrorResponse? Error)> VerifyEmailAsync(VerifyEmailRequest request);

        /// <summary>Generate a password reset token for the given email.</summary>
        Task<(object? Response, ErrorResponse? Error)> RequestPasswordResetAsync(RequestPasswordResetRequest request);

        /// <summary>Reset user password using a valid reset token.</summary>
        Task<(object? Response, ErrorResponse? Error)> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
