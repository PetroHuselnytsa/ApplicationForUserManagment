namespace TestFirstProject.DTOs
{
    /// <summary>
    /// Request DTO for user registration.
    /// </summary>
    public record RegisterRequest(string Email, string Password, string Name);

    /// <summary>
    /// Request DTO for user login.
    /// </summary>
    public record LoginRequest(string Email, string Password);

    /// <summary>
    /// Request DTO for refreshing an access token.
    /// </summary>
    public record RefreshTokenRequest(string RefreshToken);

    /// <summary>
    /// Request DTO for logging out (revoking a refresh token).
    /// </summary>
    public record LogoutRequest(string RefreshToken);

    /// <summary>
    /// Response DTO returned after successful authentication.
    /// </summary>
    public record AuthResponse(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiresAt,
        UserDto User
    );

    /// <summary>
    /// DTO representing the authenticated user's public info.
    /// </summary>
    public record UserDto(Guid Id, string Email, string Name, bool IsEmailVerified, IEnumerable<string> Roles);

    /// <summary>
    /// Structured error response for all auth endpoints.
    /// </summary>
    public record ErrorResponse(string Error, string? Detail = null);

    /// <summary>
    /// Request DTO for email verification.
    /// </summary>
    public record VerifyEmailRequest(string Token);
}
