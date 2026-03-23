using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TestFirstProject.Contexts;
using TestFirstProject.Models;
using TestFirstProject.Settings;

namespace TestFirstProject
{
    public class AuthRepository
    {
        private readonly PersonsContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthRepository(PersonsContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<(bool Success, AuthResponse? Response, string? ErrorMessage, string[]? ValidationErrors)> RegisterAsync(RegisterRequest request)
        {
            // Validate email format
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                validationErrors.Add("Email is required");
            }
            else if (!emailRegex.IsMatch(request.Email))
            {
                validationErrors.Add("Invalid email format");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                validationErrors.Add("Password is required");
            }
            else if (request.Password.Length < 6)
            {
                validationErrors.Add("Password must be at least 6 characters long");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                validationErrors.Add("Name is required");
            }
            else if (request.Name.Length > 100)
            {
                validationErrors.Add("Name must not exceed 100 characters");
            }

            if (validationErrors.Count > 0)
            {
                return (false, null, "Validation failed", validationErrors.ToArray());
            }

            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (existingUser != null)
            {
                return (false, null, "Email already registered", null);
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create user
            var user = new User(request.Email.ToLower(), passwordHash, request.Name);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate tokens
            var (accessToken, accessTokenExpiration) = GenerateAccessToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);

            var response = new AuthResponse(
                accessToken,
                refreshToken,
                accessTokenExpiration,
                new UserDto(user.Id, user.Email, user.Name)
            );

            return (true, response, null, null);
        }

        public async Task<(bool Success, AuthResponse? Response, string? ErrorMessage)> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return (false, null, "Email and password are required");
            }

            // Find user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null)
            {
                return (false, null, "Invalid email or password");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return (false, null, "Invalid email or password");
            }

            // Generate tokens
            var (accessToken, accessTokenExpiration) = GenerateAccessToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);

            var response = new AuthResponse(
                accessToken,
                refreshToken,
                accessTokenExpiration,
                new UserDto(user.Id, user.Email, user.Name)
            );

            return (true, response, null);
        }

        public async Task<(bool Success, AuthResponse? Response, string? ErrorMessage)> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return (false, null, "Refresh token is required");
            }

            // Find the refresh token
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null)
            {
                return (false, null, "Invalid refresh token");
            }

            if (storedToken.IsRevoked)
            {
                return (false, null, "Refresh token has been revoked");
            }

            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return (false, null, "Refresh token has expired");
            }

            // Revoke the old refresh token
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            // Generate new tokens
            var user = storedToken.User;
            var (accessToken, accessTokenExpiration) = GenerateAccessToken(user);
            var newRefreshToken = await GenerateRefreshTokenAsync(user.Id);

            await _context.SaveChangesAsync();

            var response = new AuthResponse(
                accessToken,
                newRefreshToken,
                accessTokenExpiration,
                new UserDto(user.Id, user.Email, user.Name)
            );

            return (true, response, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return (false, "Refresh token is required");
            }

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
            {
                return (false, "Invalid refresh token");
            }

            if (storedToken.IsRevoked)
            {
                return (true, null); // Already revoked, consider it a success
            }

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task RevokeAllUserTokensAsync(Guid userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private (string Token, DateTime Expiration) GenerateAccessToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
        }

        private async Task<string> GenerateRefreshTokenAsync(Guid userId)
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var token = Convert.ToBase64String(randomBytes);

            var refreshToken = new RefreshToken(
                userId,
                token,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
            );

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return token;
        }
    }
}
