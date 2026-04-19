using TestFirstProject.DTOs.Auth;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Handles user registration, login, and JWT token generation.
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}
