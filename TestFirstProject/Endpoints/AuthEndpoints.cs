using TestFirstProject.DTOs;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    /// <summary>
    /// Maps authentication endpoints: register and login.
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
        }
    }
}
