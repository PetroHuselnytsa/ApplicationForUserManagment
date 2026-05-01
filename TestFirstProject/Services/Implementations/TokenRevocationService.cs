using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.Models;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    /// <summary>
    /// Persists and checks revoked JWT IDs using the database.
    /// Double-revocation (same JTI) is silently ignored due to the unique index.
    /// </summary>
    public class TokenRevocationService : ITokenRevocationService
    {
        private readonly PersonsContext _context;

        public TokenRevocationService(PersonsContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task RevokeAsync(string jti, DateTime expiry)
        {
            // Guard against double-logout — if the JTI is already revoked just return
            var alreadyRevoked = await _context.RevokedTokens.AnyAsync(t => t.Jti == jti);
            if (alreadyRevoked)
                return;

            _context.RevokedTokens.Add(new RevokedToken
            {
                Id = Guid.NewGuid(),
                Jti = jti,
                ExpiresAt = expiry,
                RevokedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<bool> IsRevokedAsync(string jti)
        {
            return await _context.RevokedTokens.AnyAsync(t => t.Jti == jti);
        }
    }
}
