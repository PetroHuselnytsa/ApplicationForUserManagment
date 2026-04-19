using System.Text;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TestFirstProject;
using TestFirstProject.Contexts;
using TestFirstProject.Endpoints;
using TestFirstProject.Endpoints.Game;
using TestFirstProject.Hubs;
using TestFirstProject.Middleware;
using TestFirstProject.Services.Implementations;
using TestFirstProject.Services.Implementations.Game;
using TestFirstProject.Services.Interfaces;
using TestFirstProject.Services.Interfaces.Game;
using TestFirstProject.Settings;

var builder = WebApplication.CreateBuilder(args);

// --- Database ---
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
builder.Services.AddDbContext<PersonsContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(connectionString));

// Existing services
builder.Services.AddScoped<OperationsRepository>();

// --- JWT Authentication ---
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException($"Missing '{JwtSettings.SectionName}' configuration section.");
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.Zero
    };

    // SignalR: read JWT from query string for WebSocket connections
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            // Only read token from query string for hub paths
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// --- SignalR ---
builder.Services.AddSignalR();

// --- MediatR (domain events) ---
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// --- Application Services ---
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISignalRNotifier, SignalRNotifier>();

// --- RPG Game Engine Services ---
builder.Services.AddScoped<ICharacterProgressionService, CharacterProgressionService>();
builder.Services.AddScoped<IDamageCalculator, DamageCalculator>();
builder.Services.AddScoped<IStatusEffectProcessor, StatusEffectProcessor>();
builder.Services.AddScoped<ICombatEngine, CombatEngine>();
builder.Services.AddScoped<ILootService, LootService>();
builder.Services.AddScoped<IDungeonRunner, DungeonRunner>();
builder.Services.AddScoped<IQuestProgressTracker, QuestProgressTracker>();

// --- Rate Limiting ---
builder.Services.AddRateLimiter(options =>
{
    // Rate limit for message sending: 30 messages per minute per user
    options.AddPolicy("messaging", httpContext =>
    {
        var userId = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? httpContext.Connection.RemoteIpAddress?.ToString()
                     ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        await ExceptionHandlingMiddleware.WriteErrorResponse(
            context.HttpContext, 429, "Rate limit exceeded. Maximum 30 messages per minute.");
    };
});

var app = builder.Build();

// --- Middleware Pipeline ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

// --- Existing Person/User endpoints (preserved as-is) ---
app.MapGet("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.GetPersons(request, response);
});
app.MapPost("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.CreatePerson(response, request);
});
app.MapPut("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.UpdatePerson(response, request);
});
app.MapDelete("/api/users/{id}", async (string id, OperationsRepository operations, HttpResponse response) =>
{
    if (!RegexPatterns.IdFormatRegex().IsMatch(id))
    {
        response.StatusCode = 400;
        await response.WriteAsync("Invalid ID format. Expected format: xxx-xxxx-xxxx");
        return;
    }

    await operations.DeletePerson(response, id);
});

// --- New endpoints: Auth, Conversations, Notifications ---
app.MapAuthEndpoints();
app.MapConversationEndpoints();
app.MapNotificationEndpoints();

// --- RPG Game Engine endpoints ---
app.MapCharacterEndpoints();
app.MapBattleEndpoints();
app.MapInventoryEndpoints();
app.MapDungeonEndpoints();
app.MapQuestEndpoints();

// --- SignalR Hub ---
app.MapHub<ChatHub>("/hubs/chat");

// --- Fallback for SPA ---
app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();

/// <summary>Source-generated regex for user ID validation, avoiding per-request Regex allocation.</summary>
internal static partial class RegexPatterns
{
    [GeneratedRegex(@"^[A-Fa-f0-9]{3}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}$")]
    internal static partial Regex IdFormatRegex();
}
