using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TestFirstProject;
using TestFirstProject.Contexts;
using TestFirstProject.Hubs;
using TestFirstProject.Middleware;
using TestFirstProject.Services;
using TestFirstProject.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<PersonsContext>();

// ─── Existing services ───────────────────────────────────────────────────────
builder.Services.AddScoped<OperationsRepository>();

// ─── JWT Authentication ──────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json.");

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Allow SignalR to receive JWT via query string for WebSocket connections
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

builder.Services.AddAuthorization();

// ─── Controllers & SignalR ───────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddSignalR();

// ─── Application Services (DI) ──────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISignalRNotifier, SignalRNotifier>();

// ─── Rate Limiter (singleton — in-memory state shared across requests) ──────
builder.Services.AddSingleton<IMessageRateLimiter, MessageRateLimiter>();

// ─── Background Notification Queue ──────────────────────────────────────────
var notificationChannel = Channel.CreateUnbounded<NotificationRequest>(
    new UnboundedChannelOptions { SingleReader = true });
builder.Services.AddSingleton(notificationChannel);
builder.Services.AddSingleton<INotificationQueue, NotificationQueue>();
builder.Services.AddHostedService<BackgroundNotificationService>();

// ─── CORS (allow all origins for development; restrict in production) ────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // Named policy for SignalR — must allow credentials
    options.AddPolicy("SignalR", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ─── Middleware Pipeline ─────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// ─── Map Controllers & SignalR Hub ───────────────────────────────────────────
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat").RequireCors("SignalR");

// ─── Existing Minimal API Endpoints (preserved as-is) ───────────────────────
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
    var regex = new Regex(@"^[A-Fa-f0-9]{3}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}$");
    if (!regex.IsMatch(id))
    {
        response.StatusCode = 400;
        await response.WriteAsync("Invalid ID format. Expected format: xxx-xxxx-xxxx");
        return;
    }

    await operations.DeletePerson(response, id);
});
app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();
