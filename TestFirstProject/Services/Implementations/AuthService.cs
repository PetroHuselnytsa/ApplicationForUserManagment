using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    /// <summary>
    /// Handles user registration and login with BCrypt password hashing.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly PersonsContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(PersonsContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length > 50)
                throw new ValidationException("Username is required and must be at most 50 characters.");

            if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
                throw new ValidationException("A valid email address is required.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                throw new ValidationException("Password must be at least 8 characters.");

            // Check for existing user in a single query
            var existing = await _context.AppUsers
                .Where(u => u.Email == request.Email || u.Username == request.Username)
                .Select(u => new { u.Email, u.Username })
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                if (string.Equals(existing.Email, request.Email, StringComparison.OrdinalIgnoreCase))
                    throw new ConflictException("A user with this email already exists.");

                throw new ConflictException("A user with this username already exists.");
            }

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);
            return new AuthResponse(token, user.Id, user.Username, user.Role.ToString());
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("Email and password are required.");

            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedException("Invalid email or password.");

            var token = _tokenService.GenerateToken(user);
            return new AuthResponse(token, user.Id, user.Username, user.Role.ToString());
        }

        private static bool IsValidEmail(string email)
        {
            return System.Net.Mail.MailAddress.TryCreate(email, out _);
        }
    }
}
