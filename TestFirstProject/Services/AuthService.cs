using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Auth;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Handles user registration, authentication, and JWT token generation.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly PersonsContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(PersonsContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Check for duplicate username or email
            bool usernameExists = await _context.Users.AnyAsync(u => u.Username == request.Username);
            if (usernameExists)
                throw new BadRequestException("Username is already taken.");

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (emailExists)
                throw new BadRequestException("Email is already registered.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCryptHash(request.Password),
                Role = Models.Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                throw new BadRequestException("Invalid username or password.");

            return GenerateAuthResponse(user);
        }

        /// <summary>
        /// Generate a JWT token and wrap it in an AuthResponse.
        /// </summary>
        private AuthResponse GenerateAuthResponse(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");
            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role.ToString(),
                ExpiresAt = expiresAt
            };
        }

        /// <summary>
        /// Hash a password using a simple HMACSHA256-based approach.
        /// In production, consider using BCrypt or Argon2.
        /// </summary>
        private static string BCryptHash(string password)
        {
            // Using PBKDF2 via built-in .NET crypto for password hashing
            byte[] salt = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
                password, salt, 100000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            // Combine salt + hash for storage
            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verify a password against its stored hash.
        /// </summary>
        private static bool VerifyPassword(string password, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
                password, salt, 100000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                    return false;
            }

            return true;
        }
    }
}
