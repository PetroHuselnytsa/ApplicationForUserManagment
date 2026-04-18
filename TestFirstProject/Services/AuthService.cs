using Microsoft.EntityFrameworkCore;
using TestFirstProject.Configurations;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Models;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Implements authentication operations including registration, login, token refresh,
    /// logout, and email verification. Includes account lockout for brute-force protection.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly PersonsContext _context;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        // Account lockout defaults (can be overridden via JwtSettings config)
        private const int DefaultMaxFailedAttempts = 5;
        private const int DefaultLockoutMinutes = 15;
        private const int DefaultRefreshTokenDays = 7;
        private const int DefaultEmailVerificationHours = 24;

        public AuthService(
            PersonsContext context,
            ITokenService tokenService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(TokenResponse? Token, ErrorResponse? Error)> RegisterAsync(RegisterRequest request)
        {
            // Check for existing user with same email
            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (existingUser != null)
            {
                return (null, new ErrorResponse(409, "A user with this email already exists."));
            }

            // Hash the password using BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

            // Generate email verification token
            var verificationToken = Guid.NewGuid().ToString("N");
            var emailVerificationHours = int.Parse(
                _configuration["JwtSettings:EmailVerificationHours"] ?? DefaultEmailVerificationHours.ToString());

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.ToLower().Trim(),
                PasswordHash = passwordHash,
                EmailVerified = false,
                EmailVerificationToken = verificationToken,
                EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(emailVerificationHours),
                FailedLoginAttempts = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            // Assign default "User" role
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = RoleConfiguration.UserRoleId
            };
            _context.UserRoles.Add(userRole);

            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: {Email}. Verification token: {Token}",
                user.Email, verificationToken);

            // Generate tokens
            var roles = new[] { "User" };
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Store refresh token in database
            var refreshTokenDays = int.Parse(
                _configuration["JwtSettings:RefreshTokenExpirationDays"] ?? DefaultRefreshTokenDays.ToString());
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
                CreatedAt = DateTime.UtcNow
            };
            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return (new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = _tokenService.GetAccessTokenExpiration()
            }, null);
        }

        public async Task<(TokenResponse? Token, ErrorResponse? Error)> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null)
            {
                return (null, new ErrorResponse(401, "Invalid email or password."));
            }

            // Check if account is locked out
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var remainingMinutes = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                _logger.LogWarning("Login attempt for locked account: {Email}", user.Email);
                return (null, new ErrorResponse(429,
                    $"Account is locked due to multiple failed login attempts. Try again in {remainingMinutes} minute(s)."));
            }

            // Verify password
            var maxFailedAttempts = int.Parse(
                _configuration["JwtSettings:MaxFailedAttempts"] ?? DefaultMaxFailedAttempts.ToString());
            var lockoutMinutes = int.Parse(
                _configuration["JwtSettings:LockoutMinutes"] ?? DefaultLockoutMinutes.ToString());

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                // Increment failed attempts
                user.FailedLoginAttempts++;
                user.UpdatedAt = DateTime.UtcNow;

                if (user.FailedLoginAttempts >= maxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);
                    _logger.LogWarning("Account locked after {Attempts} failed attempts: {Email}",
                        user.FailedLoginAttempts, user.Email);
                }

                await _context.SaveChangesAsync();
                return (null, new ErrorResponse(401, "Invalid email or password."));
            }

            // Reset failed login attempts on successful login
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            user.UpdatedAt = DateTime.UtcNow;

            // Get user roles
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Store refresh token
            var refreshTokenDaysLogin = int.Parse(
                _configuration["JwtSettings:RefreshTokenExpirationDays"] ?? DefaultRefreshTokenDays.ToString());
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDaysLogin),
                CreatedAt = DateTime.UtcNow
            };
            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged in: {Email}", user.Email);

            return (new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = _tokenService.GetAccessTokenExpiration()
            }, null);
        }

        public async Task<(TokenResponse? Token, ErrorResponse? Error)> RefreshTokenAsync(RefreshRequest request)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null)
            {
                return (null, new ErrorResponse(401, "Invalid refresh token."));
            }

            // If the token has been revoked, this could be a token reuse attack.
            // Revoke all tokens for this user as a safety measure.
            if (storedToken.IsRevoked)
            {
                _logger.LogWarning(
                    "Attempted reuse of revoked refresh token for user {UserId}. Revoking all tokens.",
                    storedToken.UserId);

                await RevokeAllUserTokensAsync(storedToken.UserId, "Revoked due to detected token reuse");
                return (null, new ErrorResponse(401, "Token has been revoked. All sessions have been terminated for security."));
            }

            if (storedToken.IsExpired)
            {
                return (null, new ErrorResponse(401, "Refresh token has expired. Please login again."));
            }

            // Rotate: revoke old token and issue new pair
            var user = storedToken.User;
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Revoke the old token and link to the new one
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.ReplacedByToken = newRefreshToken;

            // Create new refresh token
            var refreshTokenDaysRefresh = int.Parse(
                _configuration["JwtSettings:RefreshTokenExpirationDays"] ?? DefaultRefreshTokenDays.ToString());
            var newRefreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDaysRefresh),
                CreatedAt = DateTime.UtcNow
            };
            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            return (new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiresAt = _tokenService.GetAccessTokenExpiration()
            }, null);
        }

        public async Task<ErrorResponse?> LogoutAsync(LogoutRequest request)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null)
            {
                return new ErrorResponse(400, "Invalid refresh token.");
            }

            if (storedToken.IsRevoked)
            {
                return new ErrorResponse(400, "Token has already been revoked.");
            }

            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged out. Token revoked for user {UserId}", storedToken.UserId);
            return null;
        }

        public async Task<ErrorResponse?> VerifyEmailAsync(VerifyEmailRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null)
            {
                return new ErrorResponse(404, "User not found.");
            }

            if (user.EmailVerified)
            {
                return new ErrorResponse(400, "Email is already verified.");
            }

            if (user.EmailVerificationToken != request.Token)
            {
                return new ErrorResponse(400, "Invalid verification token.");
            }

            if (user.EmailVerificationTokenExpiresAt.HasValue &&
                user.EmailVerificationTokenExpiresAt.Value < DateTime.UtcNow)
            {
                return new ErrorResponse(400, "Verification token has expired. Please request a new one.");
            }

            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Email verified for user: {Email}", user.Email);
            return null;
        }

        /// <summary>
        /// Revokes all active refresh tokens for a user (security measure against token reuse attacks).
        /// </summary>
        private async Task RevokeAllUserTokensAsync(Guid userId, string reason)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.ReplacedByToken = reason;
            }

            await _context.SaveChangesAsync();
        }
    }
}
