using System.Text;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TestFirstProject;
using TestFirstProject.Contexts;
using TestFirstProject.Models.DTOs;
using TestFirstProject.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection")
    ?? throw new InvalidOperationException(
        "PostgresConnection connection string is not configured in appsettings.json.");

builder.Services.AddDbContext<PersonsContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<OperationsRepository>();

// JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException(
        "JWT_SECRET environment variable is not set. " +
        "Set it before running: export JWT_SECRET=\"your-256-bit-secret-key-here\"");

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TestFirstProject";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TestFirstProject";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddSingleton(new JwtSettings(
    Secret: jwtSecret,
    Issuer: jwtIssuer,
    Audience: jwtAudience,
    SigningKey: signingKey,
    AccessTokenExpirationMinutes: int.TryParse(
        builder.Configuration["Jwt:AccessTokenExpirationMinutes"], out var mins) ? mins : 15
));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Allow HTTP in development
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.AdminOnly, policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy(PolicyNames.PremiumUser, policy =>
        policy.RequireRole("PremiumUser", "Admin"));

    options.AddPolicy(PolicyNames.AuthenticatedUser, policy =>
        policy.RequireAuthenticatedUser());
});

// Rate Limiting
static FixedWindowRateLimiterOptions CreateRateLimitOptions(int permitLimit) => new()
{
    PermitLimit = permitLimit,
    Window = TimeSpan.FromMinutes(1),
    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    QueueLimit = 0
};

static string GetPartitionKey(HttpContext ctx)
    => ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(PolicyNames.AuthRateLimit, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetPartitionKey(httpContext),
            factory: _ => CreateRateLimitOptions(permitLimit: 10)));

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetPartitionKey(httpContext),
            factory: _ => CreateRateLimitOptions(permitLimit: 100)));
});

// Services
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new ApiErrorResponse
            {
                Message = "Validation failed.",
                Errors = errors
            });
        };
    });

var app = builder.Build();

app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Minimal API Endpoints
app.MapGet("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.GetPersons(request, response);
}).RequireAuthorization(PolicyNames.AuthenticatedUser);

app.MapPost("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.CreatePerson(response, request);
}).RequireAuthorization(PolicyNames.AuthenticatedUser);

app.MapPut("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.UpdatePerson(response, request);
}).RequireAuthorization(PolicyNames.AuthenticatedUser);

var idPattern = new Regex(@"^[A-Fa-f0-9]{3}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}$", RegexOptions.Compiled);

app.MapDelete("/api/users/{id}", async (string id, OperationsRepository operations, HttpResponse response) =>
{
    if (!idPattern.IsMatch(id))
    {
        response.StatusCode = 400;
        await response.WriteAsync("Invalid ID format. Expected format: xxx-xxxx-xxxx");
        return;
    }

    await operations.DeletePerson(response, id);
}).RequireAuthorization(PolicyNames.AdminOnly);

app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();
