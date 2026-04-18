using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TestFirstProject.Configurations;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Models;
using TestFirstProject.Settings;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Implementation of IAuthService handling user registration, login,
    /// token refresh, logout, and email verification.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly PersonsContext _context;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        // Account lockout configuration
        private const int MaxFailedLoginAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        // Password validation: min 8 chars, at least one uppercase, lowercase, digit, special char
        private static readonly Regex PasswordRegex = new(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            RegexOptions.Compiled);

        public AuthService(
            PersonsContext context,
            ITokenService tokenService,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Validate email format
            if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
            {
                throw new ArgumentException("A valid email address is required.");
            }

            // Validate password strength
            if (string.IsNullOrWhiteSpace(request.Password) || !PasswordRegex.IsMatch(request.Password))
            {
                throw new ArgumentException(
                    "Password must be at least 8 characters and include an uppercase letter, " +
                    "a lowercase letter, a digit, and a special character.");
            }

            // Validate name
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Name is required.");
            }

            // Check email uniqueness
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == normalizedEmail);

            if (emailExists)
            {
                throw new InvalidOperationException("An account with this email already exists.");
            }

            // Hash the password using BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

            // Generate email verification token
            var verificationToken = GenerateSecureToken();

            // Create the new user
            var user = new User(normalizedEmail, passwordHash, request.Name.Trim())
            {
                EmailVerificationToken = verificationToken,
                EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _context.Users.Add(user);

            // Assign default "User" role
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = RoleConfiguration.UserRoleId
            };
            _context.UserRoles.Add(userRole);

            // Create refresh token
            var refreshTokenStr = _tokenService.GenerateRefreshToken();
            var refreshToken = new RefreshToken(
                refreshTokenStr,
                user.Id,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));

            _context.RefreshTokens.Add(refreshToken);

            await _context.SaveChangesAsync();

            _logger.LogInformation("New user registered: {Email} (verification token generated)", normalizedEmail);

            // Generate access token
            var roles = new[] { "User" };
            var accessToken = _tokenService.GenerateAccessToken(user, roles);

            return new AuthResponse(
                AccessToken: accessToken,
                RefreshToken: refreshTokenStr,
                AccessTokenExpiresAt: _tokenService.GetAccessTokenExpiration(),
                User: MapToDto(user, roles)
            );
        }

        /// <inheritdoc />
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Email and password are required.");
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            // Load user with roles
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // Check account lockout
            if (user.LockoutEndAt.HasValue && user.LockoutEndAt.Value > DateTime.UtcNow)
            {
                var remaining = user.LockoutEndAt.Value - DateTime.UtcNow;
                throw new UnauthorizedAccessException(
                    $"Account is locked. Try again in {Math.Ceiling(remaining.TotalMinutes)} minute(s).");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                // Increment failed login attempts
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= MaxFailedLoginAttempts)
                {
                    user.LockoutEndAt = DateTime.UtcNow.Add(LockoutDuration);
                    _logger.LogWarning("Account locked for {Email} after {Attempts} failed attempts",
                        normalizedEmail, user.FailedLoginAttempts);
                }

                await _context.SaveChangesAsync();
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // Reset failed login attempts on successful login
            user.FailedLoginAttempts = 0;
            user.LockoutEndAt = null;

            // Create new refresh token
            var refreshTokenStr = _tokenService.GenerateRefreshToken();
            var refreshToken = new RefreshToken(
                refreshTokenStr,
                user.Id,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            // Generate access token with user roles
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var accessToken = _tokenService.GenerateAccessToken(user, roles);

            _logger.LogInformation("User logged in: {Email}", normalizedEmail);

            return new AuthResponse(
                AccessToken: accessToken,
                RefreshToken: refreshTokenStr,
                AccessTokenExpiresAt: _tokenService.GetAccessTokenExpiration(),
                User: MapToDto(user, roles)
            );
        }

        /// <inheritdoc />
        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                throw new ArgumentException("Refresh token is required.");
            }

            // Find the refresh token in the database
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            // Prevent reuse of revoked tokens
            if (storedToken.IsRevoked)
            {
                _logger.LogWarning("Attempted reuse of revoked refresh token for user {UserId}",
                    storedToken.UserId);
                throw new UnauthorizedAccessException("Refresh token has been revoked.");
            }

            // Check expiration
            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token has expired.");
            }

            // Revoke the old refresh token (token rotation)
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            // Issue new refresh token
            var newRefreshTokenStr = _tokenService.GenerateRefreshToken();
            var newRefreshToken = new RefreshToken(
                newRefreshTokenStr,
                storedToken.UserId,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            // Generate new access token
            var user = storedToken.User;
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var accessToken = _tokenService.GenerateAccessToken(user, roles);

            return new AuthResponse(
                AccessToken: accessToken,
                RefreshToken: newRefreshTokenStr,
                AccessTokenExpiresAt: _tokenService.GetAccessTokenExpiration(),
                User: MapToDto(user, roles)
            );
        }

        /// <inheritdoc />
        public async Task LogoutAsync(LogoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                throw new ArgumentException("Refresh token is required.");
            }

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            if (!storedToken.IsRevoked)
            {
                storedToken.IsRevoked = true;
                storedToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Refresh token revoked for user {UserId}", storedToken.UserId);
        }

        /// <inheritdoc />
        public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                throw new ArgumentException("Verification token is required.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == request.Token);

            if (user == null)
            {
                throw new ArgumentException("Invalid verification token.");
            }

            if (user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            {
                throw new ArgumentException("Verification token has expired.");
            }

            if (user.IsEmailVerified)
            {
                return true; // Already verified
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Email verified for user {Email}", user.Email);
            return true;
        }

        /// <summary>
        /// Maps a User entity to a UserDto for API responses.
        /// </summary>
        private static UserDto MapToDto(User user, IEnumerable<string> roles)
        {
            return new UserDto(
                Id: user.Id,
                Email: user.Email,
                Name: user.Name,
                IsEmailVerified: user.IsEmailVerified,
                Roles: roles
            );
        }

        /// <summary>
        /// Basic email format validation.
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a cryptographically secure random token string for email verification.
        /// </summary>
        private static string GenerateSecureToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }
}
