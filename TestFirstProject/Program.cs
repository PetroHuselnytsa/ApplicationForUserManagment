using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TestFirstProject;
using TestFirstProject.Constants;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Endpoints;
using TestFirstProject.Hubs;
using TestFirstProject.Middleware;
using TestFirstProject.Services;
using TestFirstProject.Services.Interfaces;
using TestFirstProject.Settings;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

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
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISignalRNotifier, SignalRNotifier>();

// Bounded channel provides backpressure under load
builder.Services.AddSingleton(Channel.CreateBounded<NotificationEvent>(
    new BoundedChannelOptions(1000)
    {
        SingleReader = true,
        FullMode = BoundedChannelFullMode.DropOldest
    }));
builder.Services.AddHostedService<NotificationBackgroundService>();

builder.Services.AddSignalR();

// ---------------------------------------------------------------------------
// Authentication
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

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// ---------------------------------------------------------------------------
// Authorization
// ---------------------------------------------------------------------------

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.AdminOnly, policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy(PolicyNames.PremiumUser, policy =>
        policy.RequireRole("PremiumUser", "Admin"));

    options.AddPolicy(PolicyNames.UserManagement, policy =>
        policy.RequireRole("User", "PremiumUser", "Admin"));
});

// ---------------------------------------------------------------------------
// Rate Limiting
// ---------------------------------------------------------------------------

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(PolicyNames.AuthRateLimit, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy(PolicyNames.MessageRateLimit, httpContext =>
    {
        var userId = httpContext.User?.FindFirst(
            System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"msg_{userId}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});

// ---------------------------------------------------------------------------
// Build App
// ---------------------------------------------------------------------------

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRateLimiter();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// ---------------------------------------------------------------------------
// Auth Endpoints
// ---------------------------------------------------------------------------
// Exception handling is done by ExceptionHandlingMiddleware — no try-catch needed.

var authGroup = app.MapGroup("/auth")
    .RequireRateLimiting(PolicyNames.AuthRateLimit);

authGroup.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
{
    var response = await authService.RegisterAsync(request);
    return Results.Created("/auth/register", response);
});

authGroup.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
{
    var response = await authService.LoginAsync(request);
    return Results.Ok(response);
});

authGroup.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
{
    var response = await authService.RefreshTokenAsync(request);
    return Results.Ok(response);
});

authGroup.MapPost("/logout", async (LogoutRequest request, IAuthService authService) =>
{
    await authService.LogoutAsync(request);
    return Results.Ok(new { message = "Logged out successfully." });
}).RequireAuthorization();

authGroup.MapPost("/verify-email", async (VerifyEmailRequest request, IAuthService authService) =>
{
    await authService.VerifyEmailAsync(request);
    return Results.Ok(new { message = "Email verified successfully." });
});

// ---------------------------------------------------------------------------
// Person Endpoints
// ---------------------------------------------------------------------------

app.MapGet("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.GetPersons(request, response);
}).RequireAuthorization(PolicyNames.UserManagement);

app.MapPost("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.CreatePerson(response, request);
}).RequireAuthorization(PolicyNames.UserManagement);

app.MapPut("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.UpdatePerson(response, request);
}).RequireAuthorization(PolicyNames.UserManagement);

app.MapDelete("/api/users/{id}", async (string id, OperationsRepository operations, HttpResponse response) =>
{
    if (!IdFormatRegex().IsMatch(id))
    {
        response.StatusCode = 400;
        await response.WriteAsync("Invalid ID format. Expected format: xxx-xxxx-xxxx");
        return;
    }

    await operations.DeletePerson(response, id);
}).RequireAuthorization(PolicyNames.AdminOnly);

// ---------------------------------------------------------------------------
// Messaging & Notification Endpoints
// ---------------------------------------------------------------------------

app.MapConversationEndpoints();
app.MapNotificationEndpoints();

app.MapHub<ChatHub>("/hubs/chat");

// ---------------------------------------------------------------------------
// Fallback
// ---------------------------------------------------------------------------

app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();

// Source-generated regex avoids per-request allocation
partial class Program
{
    [GeneratedRegex(@"^[A-Fa-f0-9]{3}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}$")]
    private static partial Regex IdFormatRegex();
}
