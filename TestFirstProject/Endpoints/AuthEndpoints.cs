using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TestFirstProject.DTOs;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    /// <summary>
    /// Maps authentication endpoints: register, login, and logout.
    /// </summary>
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth")
                           .WithTags("Authentication");

            group.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
            {
                var result = await authService.RegisterAsync(request);
                return Results.Created($"/api/auth/users/{result.UserId}", result);
            })
            .WithName("Register")
            .AllowAnonymous();

            group.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
            {
                var result = await authService.LoginAsync(request);
                return Results.Ok(result);
            })
            .WithName("Login")
            .AllowAnonymous();

            // Requires a valid Bearer token; revokes it so subsequent requests with the same token are rejected.
            group.MapPost("/logout", async (HttpContext httpContext, IAuthService authService) =>
            {
                var jti = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
                if (string.IsNullOrEmpty(jti))
                    return Results.Unauthorized();

                // Parse the token expiry from the standard "exp" claim (Unix timestamp)
                var expClaim = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Exp);
                var tokenExpiry = expClaim is not null && long.TryParse(expClaim, out var expUnix)
                    ? DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime
                    : DateTime.UtcNow;

                await authService.LogoutAsync(jti, tokenExpiry);
                return Results.NoContent();
            })
            .WithName("Logout")
            .RequireAuthorization();
        }
    }
}
