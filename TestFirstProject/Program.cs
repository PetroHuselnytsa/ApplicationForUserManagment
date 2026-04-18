using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using TestFirstProject;
using TestFirstProject.Contexts;
using TestFirstProject.Models;
using TestFirstProject.Services;
using TestFirstProject.Settings;

var builder = WebApplication.CreateBuilder(args);

// ─── Configuration ──────────────────────────────────────────
// Bind JwtSettings from appsettings.json, allowing override via environment variable
var jwtSettingsSection = builder.Configuration.GetSection(JwtSettings.SectionName);
builder.Services.Configure<JwtSettings>(jwtSettingsSection);

var jwtSettings = jwtSettingsSection.Get<JwtSettings>()!;

// Allow overriding the JWT secret key via environment variable (production best practice)
var envSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
if (!string.IsNullOrWhiteSpace(envSecretKey))
{
    jwtSettings.SecretKey = envSecretKey;
}

// ─── Services ───────────────────────────────────────────────
builder.Services.AddDbContext<PersonsContext>();
builder.Services.AddScoped<OperationsRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ─── Authentication ─────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // No clock skew for strict expiration
    };

    // Return structured JSON error responses for auth failures
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var error = new ErrorResponse
            {
                StatusCode = 401,
                Message = "Authentication required. Please provide a valid access token."
            };
            return context.Response.WriteAsJsonAsync(error);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            var error = new ErrorResponse
            {
                StatusCode = 403,
                Message = "You do not have permission to access this resource."
            };
            return context.Response.WriteAsJsonAsync(error);
        }
    };
});

// ─── Authorization Policies ─────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    // Policy: Only Admin role
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Policy: Premium users (any role, but must have IsPremium claim = true)
    options.AddPolicy("PremiumUser", policy =>
        policy.RequireClaim("IsPremium", "true"));

    // Policy: Any authenticated user
    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

// ─── Rate Limiting ──────────────────────────────────────────
// Protect auth endpoints against brute force attacks
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    // Fixed window: max 10 requests per minute per IP for auth endpoints
    options.AddPolicy("AuthRateLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Stricter limit for login endpoint (5 requests per minute)
    options.AddPolicy("LoginRateLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

var app = builder.Build();

// ─── Middleware Pipeline (order matters) ─────────────────────
app.UseRateLimiter();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// ═══════════════════════════════════════════════════════════════
// AUTH ENDPOINTS
// ═══════════════════════════════════════════════════════════════

app.MapPost("/auth/register", async (RegisterRequest request, IAuthService authService) =>
{
    var (response, error) = await authService.RegisterAsync(request);
    if (error != null)
        return Results.Json(error, statusCode: error.StatusCode);

    return Results.Json(response, statusCode: 201);
})
.RequireRateLimiting("AuthRateLimit");

app.MapPost("/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    var (response, error) = await authService.LoginAsync(request);
    if (error != null)
        return Results.Json(error, statusCode: error.StatusCode);

    return Results.Ok(response);
})
.RequireRateLimiting("LoginRateLimit");

app.MapPost("/auth/refresh", async (RefreshRequest request, IAuthService authService) =>
{
    var (response, error) = await authService.RefreshAsync(request);
    if (error != null)
        return Results.Json(error, statusCode: error.StatusCode);

    return Results.Ok(response);
})
.RequireRateLimiting("AuthRateLimit");

app.MapPost("/auth/logout", async (LogoutRequest request, IAuthService authService) =>
{
    var error = await authService.LogoutAsync(request);
    if (error != null)
        return Results.Json(error, statusCode: error.StatusCode);

    return Results.Ok(new { message = "Logged out successfully." });
})
.RequireRateLimiting("AuthRateLimit");

// ─── Bonus: Email Verification ──────────────────────────────

app.MapPost("/auth/verify-email", async (VerifyEmailRequest request, IAuthService authService) =>
{
    var (response, error) = await authService.VerifyEmailAsync(request);
    if (error != null)
        return Results.Json(error, statusCode: error.StatusCode);

    return Results.Ok(response);
})
.RequireRateLimiting("AuthRateLimit");

// ─── Bonus: Password Reset ─────────────────────────────────

app.MapPost("/auth/request-password-reset", async (RequestPasswordResetRequest request, IAuthService authService) =>
{
    var (response, error) = await authService.RequestPasswordResetAsync(request);
    if (error != null)
        return Results.Json(error, statusCode: error.StatusCode);

    return Results.Ok(response);
})
.RequireRateLimiting("AuthRateLimit");

app.MapPost("/auth/reset-password", async (ResetPasswordRequest request, IAuthService authService) =>
{
    var (response, error) = await authService.ResetPasswordAsync(request);
    if (error != null)
        return Results.Json(error, statusCode: error.StatusCode);

    return Results.Ok(response);
})
.RequireRateLimiting("AuthRateLimit");

// ═══════════════════════════════════════════════════════════════
// EXISTING USER/PERSON ENDPOINTS (now protected with authorization)
// ═══════════════════════════════════════════════════════════════

app.MapGet("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.GetPersons(request, response);
})
.RequireAuthorization("AuthenticatedUser");

app.MapPost("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.CreatePerson(response, request);
})
.RequireAuthorization("AuthenticatedUser");

app.MapPut("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.UpdatePerson(response, request);
})
.RequireAuthorization("AuthenticatedUser");

app.MapDelete("/api/users/{id}", async (string id, OperationsRepository operations, HttpResponse response) =>
{
    var regex = new Regex(@"^[A-Fa-f0-9]{3}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}$");
    if (!regex.IsMatch(id))
    {
        response.StatusCode = 400;
        await response.WriteAsync("Invalid ID format. Expected format: xxx-xxxx-xxxx");
        return;
    }

    await operations.DeletePerson(response, id);
})
.RequireAuthorization("AdminOnly");

app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();
