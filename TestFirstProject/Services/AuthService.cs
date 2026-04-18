using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TestFirstProject.Contexts;
using TestFirstProject.Models;
using TestFirstProject.Settings;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Implements JWT-based authentication with refresh token rotation,
    /// role-based authorization support, account lockout, email verification,
    /// and password reset flows.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly PersonsContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        // Lockout policy: 5 failed attempts → 15 minute lockout
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        // Token expiration for email verification and password reset
        private static readonly TimeSpan EmailTokenExpiration = TimeSpan.FromHours(24);
        private static readonly TimeSpan PasswordResetTokenExpiration = TimeSpan.FromHours(1);

        public AuthService(
            PersonsContext context,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        // ─── REGISTRATION ───────────────────────────────────────────

        public async Task<(AuthResponse? Response, ErrorResponse? Error)> RegisterAsync(RegisterRequest request)
        {
            // Validate password strength
            var passwordErrors = ValidatePasswordStrength(request.Password);
            if (passwordErrors.Count > 0)
            {
                return (null, new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Password does not meet security requirements.",
                    Errors = passwordErrors
                });
            }

            // Check email uniqueness
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (emailExists)
            {
                return (null, new ErrorResponse
                {
                    StatusCode = 409,
                    Message = "A user with this email already exists."
                });
            }

            // Create user with BCrypt-hashed password
            var user = new User(
                email: request.Email.ToLower().Trim(),
                passwordHash: BCrypt.Net.BCrypt.HashPassword(request.Password),
                name: request.Name.Trim()
            )
            {
                Id = Guid.NewGuid(),
                Role = Role.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            // Generate email verification token
            var verificationToken = new EmailVerificationToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = GenerateSecureToken(),
                ExpiresAt = DateTime.UtcNow.Add(EmailTokenExpiration),
                CreatedAt = DateTime.UtcNow
            };
            _context.EmailVerificationTokens.Add(verificationToken);

            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered: {Email} (verification token: {Token})",
                user.Email, verificationToken.Token);

            // Generate tokens and return auth response
            var authResponse = await GenerateAuthResponseAsync(user);
            return (authResponse, null);
        }

        // ─── LOGIN ──────────────────────────────────────────────────

        public async Task<(AuthResponse? Response, ErrorResponse? Error)> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower().Trim());

            if (user == null)
            {
                return (null, new ErrorResponse
                {
                    StatusCode = 401,
                    Message = "Invalid email or password."
                });
            }

            // Check account lockout
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var remaining = user.LockoutEnd.Value - DateTime.UtcNow;
                return (null, new ErrorResponse
                {
                    StatusCode = 429,
                    Message = $"Account is locked. Try again in {Math.Ceiling(remaining.TotalMinutes)} minute(s)."
                });
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                // Increment failed attempts
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                    _logger.LogWarning("Account locked due to {Count} failed attempts: {Email}",
                        user.FailedLoginAttempts, user.Email);
                }

                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return (null, new ErrorResponse
                {
                    StatusCode = 401,
                    Message = "Invalid email or password."
                });
            }

            // Reset failed login attempts on successful login
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var authResponse = await GenerateAuthResponseAsync(user);
            _logger.LogInformation("User logged in: {Email}", user.Email);

            return (authResponse, null);
        }

        // ─── REFRESH TOKEN ─────────────────────────────────────────

        public async Task<(AuthResponse? Response, ErrorResponse? Error)> RefreshAsync(RefreshRequest request)
        {
            // Hash the incoming token to look it up
            string tokenHash = HashToken(request.RefreshToken);

            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            if (storedToken == null)
            {
                return (null, new ErrorResponse
                {
                    StatusCode = 401,
                    Message = "Invalid refresh token."
                });
            }

            // Check if token was already revoked (potential token reuse attack)
            if (storedToken.IsRevoked)
            {
                // Revoke ALL tokens for this user as a security measure
                _logger.LogWarning(
                    "Refresh token reuse detected for user {UserId}. Revoking all tokens.",
                    storedToken.UserId);

                await RevokeAllUserTokensAsync(storedToken.UserId);

                return (null, new ErrorResponse
                {
                    StatusCode = 401,
                    Message = "Token has been revoked. Please log in again."
                });
            }

            // Check expiration
            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                storedToken.IsRevoked = true;
                storedToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return (null, new ErrorResponse
                {
                    StatusCode = 401,
                    Message = "Refresh token has expired. Please log in again."
                });
            }

            // Revoke current token (rotation: old token becomes invalid)
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Issue new tokens
            var authResponse = await GenerateAuthResponseAsync(storedToken.User);
            return (authResponse, null);
        }

        // ─── LOGOUT ─────────────────────────────────────────────────

        public async Task<ErrorResponse?> LogoutAsync(LogoutRequest request)
        {
            string tokenHash = HashToken(request.RefreshToken);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            if (storedToken == null || storedToken.IsRevoked)
            {
                return new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Invalid or already revoked token."
                };
            }

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token revoked for user {UserId}", storedToken.UserId);
            return null;
        }

        // ─── EMAIL VERIFICATION ─────────────────────────────────────

        public async Task<(object? Response, ErrorResponse? Error)> VerifyEmailAsync(VerifyEmailRequest request)
        {
            var token = await _context.EmailVerificationTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == request.Token);

            if (token == null)
            {
                return (null, new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Invalid verification token."
                });
            }

            if (token.IsUsed)
            {
                return (null, new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "This verification token has already been used."
                });
            }

            if (token.ExpiresAt < DateTime.UtcNow)
            {
                return (null, new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Verification token has expired."
                });
            }

            // Mark token as used and verify user email
            token.IsUsed = true;
            token.User.IsEmailVerified = true;
            token.User.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Email verified for user {Email}", token.User.Email);

            return (new { message = "Email verified successfully." }, null);
        }

        // ─── PASSWORD RESET REQUEST ─────────────────────────────────

        public async Task<(object? Response, ErrorResponse? Error)> RequestPasswordResetAsync(RequestPasswordResetRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower().Trim());

            // Always return success to prevent email enumeration
            var successMessage = new { message = "If the email exists, a password reset link has been sent." };

            if (user == null)
            {
                return (successMessage, null);
            }

            // Invalidate any existing unused reset tokens for this user
            var existingTokens = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed)
                .ToListAsync();

            foreach (var existing in existingTokens)
            {
                existing.IsUsed = true;
            }

            // Create new password reset token
            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = GenerateSecureToken(),
                ExpiresAt = DateTime.UtcNow.Add(PasswordResetTokenExpiration),
                CreatedAt = DateTime.UtcNow
            };
            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // In production, this token would be sent via email
            _logger.LogInformation("Password reset token generated for {Email}: {Token}",
                user.Email, resetToken.Token);

            return (successMessage, null);
        }

        // ─── PASSWORD RESET ─────────────────────────────────────────

        public async Task<(object? Response, ErrorResponse? Error)> ResetPasswordAsync(ResetPasswordRequest request)
        {
            // Validate new password strength
            var passwordErrors = ValidatePasswordStrength(request.NewPassword);
            if (passwordErrors.Count > 0)
            {
                return (null, new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "New password does not meet security requirements.",
                    Errors = passwordErrors
                });
            }

            var token = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == request.Token);

            if (token == null)
            {
                return (null, new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Invalid reset token."
                });
            }

            if (token.IsUsed)
            {
                return (null, new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "This reset token has already been used."
                });
            }

            if (token.ExpiresAt < DateTime.UtcNow)
            {
                return (null, new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Reset token has expired."
                });
            }

            // Mark token as used and update password
            token.IsUsed = true;
            token.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            token.User.UpdatedAt = DateTime.UtcNow;

            // Reset lockout state when password is reset
            token.User.FailedLoginAttempts = 0;
            token.User.LockoutEnd = null;

            // Revoke all refresh tokens for security (force re-login)
            await RevokeAllUserTokensAsync(token.UserId);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password reset for user {Email}", token.User.Email);

            return (new { message = "Password has been reset successfully." }, null);
        }

        // ─── PRIVATE HELPERS ────────────────────────────────────────

        /// <summary>
        /// Generates an AuthResponse with new access and refresh tokens.
        /// </summary>
        private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateSecureToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

            // Store refresh token hash in database
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = HashToken(refreshToken),
                ExpiresAt = refreshTokenExpiry,
                CreatedAt = DateTime.UtcNow
            };
            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = MapToUserDto(user)
            };
        }

        /// <summary>
        /// Generates a JWT access token containing user claims (id, email, role, premium status).
        /// </summary>
        private string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("IsPremium", user.IsPremium.ToString().ToLower()),
                new Claim("IsEmailVerified", user.IsEmailVerified.ToString().ToLower())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates a cryptographically secure random token (Base64Url-encoded).
        /// </summary>
        private static string GenerateSecureToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        /// <summary>
        /// Hashes a token using SHA-256 for secure storage.
        /// </summary>
        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Revokes all active refresh tokens for a user (security measure).
        /// </summary>
        private async Task RevokeAllUserTokensAsync(Guid userId)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Validates password meets security requirements:
        /// - At least 8 characters
        /// - Contains uppercase letter
        /// - Contains lowercase letter
        /// - Contains digit
        /// - Contains special character
        /// </summary>
        private static List<string> ValidatePasswordStrength(string password)
        {
            var errors = new List<string>();

            if (password.Length < 8)
                errors.Add("Password must be at least 8 characters long.");
            if (!password.Any(char.IsUpper))
                errors.Add("Password must contain at least one uppercase letter.");
            if (!password.Any(char.IsLower))
                errors.Add("Password must contain at least one lowercase letter.");
            if (!password.Any(char.IsDigit))
                errors.Add("Password must contain at least one digit.");
            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                errors.Add("Password must contain at least one special character.");

            return errors;
        }

        /// <summary>
        /// Maps a User entity to a safe UserDto (no sensitive fields).
        /// </summary>
        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
                IsPremium = user.IsPremium,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
