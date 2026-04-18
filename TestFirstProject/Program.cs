using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using TestFirstProject;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Services;
using TestFirstProject.Settings;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

// Bind JwtSettings from appsettings.json (supports env var overrides via JwtSettings__SecretKey, etc.)
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>()!;

// ---------------------------------------------------------------------------
// Services
// ---------------------------------------------------------------------------

builder.Services.AddDbContext<PersonsContext>();
builder.Services.AddScoped<OperationsRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ---------------------------------------------------------------------------
// Authentication — JWT Bearer
// ---------------------------------------------------------------------------

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
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.FromSeconds(30) // Tighten default 5-min clock skew
    };
});

// ---------------------------------------------------------------------------
// Authorization — Policies
// ---------------------------------------------------------------------------

builder.Services.AddAuthorization(options =>
{
    // Policy: AdminOnly — requires the "Admin" role
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Policy: PremiumUser — requires the "PremiumUser" role
    options.AddPolicy("PremiumUser", policy =>
        policy.RequireRole("PremiumUser", "Admin"));

    // Policy: UserManagement — any authenticated user with "User" or higher role
    options.AddPolicy("UserManagement", policy =>
        policy.RequireRole("User", "PremiumUser", "Admin"));
});

// ---------------------------------------------------------------------------
// Rate Limiting — Protect auth endpoints from brute force
// ---------------------------------------------------------------------------

builder.Services.AddRateLimiter(options =>
{
    // Global rate limiter rejected response
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Fixed window rate limiter for auth endpoints
    options.AddPolicy("AuthRateLimit", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// ---------------------------------------------------------------------------
// Build App
// ---------------------------------------------------------------------------

var app = builder.Build();

// Middleware pipeline order matters
app.UseRateLimiter();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// ---------------------------------------------------------------------------
// Auth Endpoints
// ---------------------------------------------------------------------------

var authGroup = app.MapGroup("/auth")
    .RequireRateLimiting("AuthRateLimit");

authGroup.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.RegisterAsync(request);
        return Results.Created("/auth/register", response);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new ErrorResponse(ex.Message));
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new ErrorResponse(ex.Message));
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

authGroup.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.LoginAsync(request);
        return Results.Ok(response);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new ErrorResponse(ex.Message));
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Json(new ErrorResponse(ex.Message), statusCode: StatusCodes.Status401Unauthorized);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

authGroup.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.RefreshTokenAsync(request);
        return Results.Ok(response);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new ErrorResponse(ex.Message));
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Json(new ErrorResponse(ex.Message), statusCode: StatusCodes.Status401Unauthorized);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

authGroup.MapPost("/logout", async (LogoutRequest request, IAuthService authService) =>
{
    try
    {
        await authService.LogoutAsync(request);
        return Results.Ok(new { message = "Logged out successfully." });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new ErrorResponse(ex.Message));
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Json(new ErrorResponse(ex.Message), statusCode: StatusCodes.Status401Unauthorized);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
}).RequireAuthorization();

// Bonus: Email verification endpoint
authGroup.MapPost("/verify-email", async (VerifyEmailRequest request, IAuthService authService) =>
{
    try
    {
        await authService.VerifyEmailAsync(request);
        return Results.Ok(new { message = "Email verified successfully." });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new ErrorResponse(ex.Message));
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// ---------------------------------------------------------------------------
// Existing User (Person) Endpoints — Now protected with authorization
// ---------------------------------------------------------------------------

app.MapGet("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.GetPersons(request, response);
}).RequireAuthorization("UserManagement");

app.MapPost("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.CreatePerson(response, request);
}).RequireAuthorization("UserManagement");

app.MapPut("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.UpdatePerson(response, request);
}).RequireAuthorization("UserManagement");

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
}).RequireAuthorization("AdminOnly");

// ---------------------------------------------------------------------------
// Fallback — Serve SPA
// ---------------------------------------------------------------------------

app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();
