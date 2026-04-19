using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using TestFirstProject.Models;

namespace TestFirstProject.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly SigningCredentials _credentials;

        public TokenService(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
            _credentials = new SigningCredentials(_jwtSettings.SigningKey, SecurityAlgorithms.HmacSha256);
        }

        public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: _credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        public string GenerateRefreshToken()
        {
            return GenerateSecureToken(byteLength: 64);
        }

        /// <summary>
        /// Generates a cryptographically secure random token encoded as Base64.
        /// </summary>
        public static string GenerateSecureToken(int byteLength, bool urlSafe = false)
        {
            var randomBytes = new byte[byteLength];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            var encoded = Convert.ToBase64String(randomBytes);
            if (urlSafe)
            {
                encoded = encoded.Replace("+", "-").Replace("/", "_").TrimEnd('=');
            }
            return encoded;
        }
    }
}
