using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.RegularExpressions;
using TestFirstProject;
using TestFirstProject.Contexts;
using TestFirstProject.Models;
using TestFirstProject.Settings;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Get JWT settings for authentication configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

// Configure JWT Authentication
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero // No tolerance for token expiration
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<PersonsContext>();
builder.Services.AddScoped<OperationsRepository>();
builder.Services.AddScoped<AuthRepository>();

var app = builder.Build();

app.UseStaticFiles();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// ==================== AUTH ENDPOINTS ====================

// POST /auth/register - User registration
app.MapPost("/auth/register", async (AuthRepository authRepo, HttpRequest request, HttpResponse response) =>
{
    try
    {
        var registerRequest = await request.ReadFromJsonAsync<RegisterRequest>();

        if (registerRequest == null)
        {
            response.StatusCode = 400;
            await response.WriteAsJsonAsync(new ErrorResponse("Invalid request body"));
            return;
        }

        var (success, authResponse, errorMessage, validationErrors) = await authRepo.RegisterAsync(registerRequest);

        if (!success)
        {
            response.StatusCode = 400;
            await response.WriteAsJsonAsync(new ErrorResponse(errorMessage!, validationErrors));
            return;
        }

        response.StatusCode = 201;
        await response.WriteAsJsonAsync(authResponse);
    }
    catch (Exception ex)
    {
        response.StatusCode = 500;
        await response.WriteAsJsonAsync(new ErrorResponse("An error occurred during registration"));
    }
});

// POST /auth/login - User login
app.MapPost("/auth/login", async (AuthRepository authRepo, HttpRequest request, HttpResponse response) =>
{
    try
    {
        var loginRequest = await request.ReadFromJsonAsync<LoginRequest>();

        if (loginRequest == null)
        {
            response.StatusCode = 400;
            await response.WriteAsJsonAsync(new ErrorResponse("Invalid request body"));
            return;
        }

        var (success, authResponse, errorMessage) = await authRepo.LoginAsync(loginRequest);

        if (!success)
        {
            response.StatusCode = 401;
            await response.WriteAsJsonAsync(new ErrorResponse(errorMessage!));
            return;
        }

        await response.WriteAsJsonAsync(authResponse);
    }
    catch (Exception ex)
    {
        response.StatusCode = 500;
        await response.WriteAsJsonAsync(new ErrorResponse("An error occurred during login"));
    }
});

// POST /auth/refresh - Refresh access token
app.MapPost("/auth/refresh", async (AuthRepository authRepo, HttpRequest request, HttpResponse response) =>
{
    try
    {
        var refreshRequest = await request.ReadFromJsonAsync<RefreshTokenRequest>();

        if (refreshRequest == null)
        {
            response.StatusCode = 400;
            await response.WriteAsJsonAsync(new ErrorResponse("Invalid request body"));
            return;
        }

        var (success, authResponse, errorMessage) = await authRepo.RefreshTokenAsync(refreshRequest);

        if (!success)
        {
            response.StatusCode = 401;
            await response.WriteAsJsonAsync(new ErrorResponse(errorMessage!));
            return;
        }

        await response.WriteAsJsonAsync(authResponse);
    }
    catch (Exception ex)
    {
        response.StatusCode = 500;
        await response.WriteAsJsonAsync(new ErrorResponse("An error occurred during token refresh"));
    }
});

// POST /auth/logout - Logout (revoke refresh token)
app.MapPost("/auth/logout", async (AuthRepository authRepo, HttpRequest request, HttpResponse response) =>
{
    try
    {
        var logoutRequest = await request.ReadFromJsonAsync<RefreshTokenRequest>();

        if (logoutRequest == null || string.IsNullOrWhiteSpace(logoutRequest.RefreshToken))
        {
            response.StatusCode = 400;
            await response.WriteAsJsonAsync(new ErrorResponse("Refresh token is required"));
            return;
        }

        var (success, errorMessage) = await authRepo.LogoutAsync(logoutRequest.RefreshToken);

        if (!success)
        {
            response.StatusCode = 400;
            await response.WriteAsJsonAsync(new ErrorResponse(errorMessage!));
            return;
        }

        await response.WriteAsJsonAsync(new { message = "Logged out successfully" });
    }
    catch (Exception ex)
    {
        response.StatusCode = 500;
        await response.WriteAsJsonAsync(new ErrorResponse("An error occurred during logout"));
    }
});

// ==================== PROTECTED USER ENDPOINTS ====================

app.MapGet("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.GetPersons(request, response);
}).RequireAuthorization();

app.MapPost("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.CreatePerson(response, request);
}).RequireAuthorization();

app.MapPut("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.UpdatePerson(response, request);
}).RequireAuthorization();

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
}).RequireAuthorization();

// ==================== FALLBACK ====================

app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();
