using TestFirstProject.Models;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Generates and validates JWT tokens for authenticated users.
    /// </summary>
    public interface ITokenService
    {
        string GenerateToken(AppUser user);
    }
}
