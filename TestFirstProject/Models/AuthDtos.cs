using System.ComponentModel.DataAnnotations;

namespace TestFirstProject.Models
{
    // ─── Request DTOs ───────────────────────────────────────────

    /// <summary>
    /// Request body for user registration.
    /// </summary>
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 50 characters.")]
        public string Name { get; set; } = null!;
    }

    /// <summary>
    /// Request body for user login.
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;
    }

    /// <summary>
    /// Request body to refresh an access token using a refresh token.
    /// </summary>
    public class RefreshRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = null!;
    }

    /// <summary>
    /// Request body for logout (revokes the provided refresh token).
    /// </summary>
    public class LogoutRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = null!;
    }

    /// <summary>
    /// Request body for email verification.
    /// </summary>
    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = null!;
    }

    /// <summary>
    /// Request body to request a password reset link.
    /// </summary>
    public class RequestPasswordResetRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;
    }

    /// <summary>
    /// Request body to reset password using a token.
    /// </summary>
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string NewPassword { get; set; } = null!;
    }

    // ─── Response DTOs ──────────────────────────────────────────

    /// <summary>
    /// Response returned after successful authentication (login or refresh).
    /// </summary>
    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime AccessTokenExpiresAt { get; set; }
        public UserDto User { get; set; } = null!;
    }

    /// <summary>
    /// A safe projection of user data (no sensitive fields).
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsPremium { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Standard error response envelope.
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = null!;
        public List<string>? Errors { get; set; }
    }
}
