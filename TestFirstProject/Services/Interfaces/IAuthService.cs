using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Handles user registration, login, and logout.
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Revokes the supplied JWT so that subsequent requests using it are rejected.
        /// </summary>
        /// <param name="jti">The unique JWT ID (jti claim) from the token being logged out.</param>
        /// <param name="tokenExpiry">The original expiry of the token, stored for future cleanup.</param>
        Task LogoutAsync(string jti, DateTime tokenExpiry);
    }
}
