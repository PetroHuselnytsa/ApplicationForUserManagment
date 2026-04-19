using System.Security.Claims;

namespace TestFirstProject.Extensions
{
    /// <summary>
    /// Shared helper for extracting the authenticated user's ID from JWT claims.
    /// Used by all endpoint classes to avoid duplicating claim-extraction logic.
    /// </summary>
    public static class HttpContextExtensions
    {
        public static Guid GetUserId(this HttpContext httpContext)
        {
            var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
                throw new UnauthorizedAccessException("User is not authenticated.");

            return userId;
        }
    }
}
