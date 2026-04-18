using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TestFirstProject.Extensions
{
    /// <summary>
    /// Shared helpers for extracting user identity from JWT claims.
    /// Eliminates duplicate claim-parsing logic across endpoints and hubs.
    /// </summary>
    public static class ClaimsExtensions
    {
        /// <summary>
        /// Extracts the user ID as a Guid from JWT claims (Sub or NameIdentifier).
        /// Returns null if no valid claim is found.
        /// </summary>
        public static Guid? GetUserGuid(this ClaimsPrincipal? user)
        {
            var claim = user?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(claim, out var userId) ? userId : null;
        }

        /// <summary>
        /// Extracts the user ID as a Guid from the HttpContext's authenticated user.
        /// Throws UnauthorizedAccessException if the identity is missing or invalid.
        /// </summary>
        public static Guid GetRequiredUserGuid(this HttpContext httpContext)
        {
            return httpContext.User.GetUserGuid()
                ?? throw new UnauthorizedAccessException("Invalid user identity.");
        }
    }
}
