using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using TestFirstProject;
using TestFirstProject.Contexts;
using TestFirstProject.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ───────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
builder.Services.AddDbContext<PersonsContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<OperationsRepository>();

// ── Authentication (JWT Bearer) ────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? jwtSettings["SecretKey"];

// Validate that a JWT secret key is available
if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException(
        "JWT secret key is not configured. Set the JWT_SECRET_KEY environment variable " +
        "or provide JwtSettings:SecretKey in appsettings.json (not recommended for production).");
}

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.FromSeconds(30) // Reduce default 5-min clock skew
    };

    // Return structured error responses for auth failures
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var response = new { statusCode = 401, message = "Authentication required. Provide a valid Bearer token." };
            return context.Response.WriteAsJsonAsync(response);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var response = new { statusCode = 403, message = "You do not have permission to access this resource." };
            return context.Response.WriteAsJsonAsync(response);
        }
    };
});

// ── Authorization (Policies) ───────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    // Policy: Only users with Admin role
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Policy: Only users with PremiumUser role
    options.AddPolicy("PremiumUser", policy =>
        policy.RequireRole("PremiumUser", "Admin"));

    // Policy: Only users who have verified their email
    options.AddPolicy("VerifiedUser", policy =>
        policy.RequireClaim("email_verified", "true"));
});

// ── Rate Limiting ──────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Fixed window rate limiter for auth endpoints to prevent brute force
    options.AddFixedWindowLimiter("AuthEndpoints", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    // Global rate limiter
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// ── Application Services ───────────────────────────────────────────────────
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ── Controllers ────────────────────────────────────────────────────────────
builder.Services.AddControllers();

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────────────────
app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// ── Map Controller Routes ──────────────────────────────────────────────────
app.MapControllers()
   .RequireRateLimiting("AuthEndpoints");

// ── Existing Minimal API Endpoints ─────────────────────────────────────────
// GET /api/users — Public: allows reading user list without authentication
app.MapGet("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.GetPersons(request, response);
});

// POST /api/users — Protected: requires authenticated user
app.MapPost("/api/users", [Authorize] async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.CreatePerson(response, request);
});

// PUT /api/users — Protected: requires authenticated user
app.MapPut("/api/users", [Authorize] async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.UpdatePerson(response, request);
});

// DELETE /api/users/{id} — Protected: requires Admin role
app.MapDelete("/api/users/{id}", [Authorize(Policy = "AdminOnly")] async (string id, OperationsRepository operations, HttpResponse response) =>
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

// ── Fallback Route ─────────────────────────────────────────────────────────
app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();
