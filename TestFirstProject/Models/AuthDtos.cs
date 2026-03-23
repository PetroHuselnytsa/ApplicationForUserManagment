namespace TestFirstProject.Models
{
    public record RegisterRequest(string Email, string Password, string Name);

    public record LoginRequest(string Email, string Password);

    public record RefreshTokenRequest(string RefreshToken);

    public record AuthResponse(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiration,
        UserDto User
    );

    public record UserDto(Guid Id, string Email, string Name);

    public record ErrorResponse(string Message, string[]? Errors = null);
}
