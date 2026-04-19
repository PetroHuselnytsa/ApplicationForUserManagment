using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TestFirstProject.Configurations;
using TestFirstProject.Contexts;
using TestFirstProject.Models;
using TestFirstProject.Models.DTOs;

namespace TestFirstProject.Services
{
    public class AuthService : IAuthService
    {
        private readonly PersonsContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
        private static readonly TimeSpan EmailVerificationTokenLifetime = TimeSpan.FromHours(24);

        private static readonly Regex UppercasePattern = new(@"[A-Z]", RegexOptions.Compiled);
        private static readonly Regex LowercasePattern = new(@"[a-z]", RegexOptions.Compiled);
        private static readonly Regex DigitPattern = new(@"\d", RegexOptions.Compiled);
        private static readonly Regex SpecialCharPattern = new(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]", RegexOptions.Compiled);

        public AuthService(PersonsContext context, ITokenService tokenService, ILogger<AuthService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var normalizedEmail = NormalizeEmail(request.Email);

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == normalizedEmail);

            if (emailExists)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            ValidatePasswordStrength(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
                EmailConfirmed = false,
                EmailVerificationToken = TokenService.GenerateSecureToken(byteLength: 32, urlSafe: true),
                EmailVerificationTokenExpiry = DateTime.UtcNow.Add(EmailVerificationTokenLifetime),
                AccessFailedCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var defaultRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == RoleConfiguration.UserRoleId)
                ?? throw new InvalidOperationException("Default 'User' role not found. Ensure database is seeded.");

            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = defaultRole.Id
            });

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: {Email}", user.Email);

            return new RegisterResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Message = "Registration successful. Please verify your email address.",
                EmailVerificationRequired = true
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var normalizedEmail = NormalizeEmail(request.Email);

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var remainingMinutes = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes + 1;
                throw new UnauthorizedAccessException(
                    $"Account is locked due to multiple failed login attempts. Try again in {remainingMinutes} minute(s).");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                user.AccessFailedCount++;
                if (user.AccessFailedCount >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                    _logger.LogWarning("Account locked for {Email} after {Attempts} failed attempts.",
                        user.Email, user.AccessFailedCount);
                }

                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (!user.EmailConfirmed)
            {
                throw new UnauthorizedAccessException("Email address has not been verified. Please verify your email before logging in.");
            }

            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            user.UpdatedAt = DateTime.UtcNow;

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user, roles);

            var refreshToken = CreateRefreshToken(_tokenService.GenerateRefreshToken(), user.Id);
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged in successfully: {Email}", user.Email);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = expiresAt
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // First fetch only the token to check validity before loading the full user graph
            var existingToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (existingToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            if (existingToken.RevokedAt.HasValue)
            {
                _logger.LogWarning(
                    "Attempted reuse of revoked refresh token for user {UserId}. Revoking all tokens.",
                    existingToken.UserId);

                var allUserTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == existingToken.UserId && !rt.RevokedAt.HasValue)
                    .ToListAsync();

                foreach (var token in allUserTokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                throw new UnauthorizedAccessException(
                    "Refresh token has been revoked. All sessions have been terminated for security.");
            }

            if (existingToken.IsExpired)
            {
                throw new UnauthorizedAccessException("Refresh token has expired. Please log in again.");
            }

            // Token is valid — now load the user with roles
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstAsync(u => u.Id == existingToken.UserId);

            existingToken.RevokedAt = DateTime.UtcNow;

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user, roles);

            var newRefreshToken = CreateRefreshToken(_tokenService.GenerateRefreshToken(), user.Id);
            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Token refreshed for user: {Email}", user.Email);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = expiresAt
            };
        }

        public async Task LogoutAsync(LogoutRequest request)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (refreshToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            if (!refreshToken.RevokedAt.HasValue)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("User logged out. Token revoked for user {UserId}.", refreshToken.UserId);
        }

        public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
        {
            var normalizedEmail = NormalizeEmail(request.Email);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            if (user.EmailConfirmed)
            {
                throw new InvalidOperationException("Email is already verified.");
            }

            if (user.EmailVerificationToken != request.Token)
            {
                throw new InvalidOperationException("Invalid verification token.");
            }

            if (user.EmailVerificationTokenExpiry.HasValue &&
                user.EmailVerificationTokenExpiry.Value < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Verification token has expired.");
            }

            user.EmailConfirmed = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Email verified for user: {Email}", user.Email);

            return true;
        }

        private static RefreshToken CreateRefreshToken(string tokenString, Guid userId) => new()
        {
            Id = Guid.NewGuid(),
            Token = tokenString,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow
        };

        private static string NormalizeEmail(string email)
            => email.Trim().ToLowerInvariant();

        private static void ValidatePasswordStrength(string password)
        {
            var errors = new List<string>();

            if (password.Length < 8)
                errors.Add("Password must be at least 8 characters long.");
            if (!UppercasePattern.IsMatch(password))
                errors.Add("Password must contain at least one uppercase letter.");
            if (!LowercasePattern.IsMatch(password))
                errors.Add("Password must contain at least one lowercase letter.");
            if (!DigitPattern.IsMatch(password))
                errors.Add("Password must contain at least one digit.");
            if (!SpecialCharPattern.IsMatch(password))
                errors.Add("Password must contain at least one special character.");

            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    "Password does not meet strength requirements: " + string.Join(" ", errors));
            }
        }
    }
}
