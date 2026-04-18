using System.Text.Json;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;

namespace TestFirstProject.Middleware
{
    /// <summary>
    /// Global exception handling middleware that converts exceptions to structured error responses.
    /// Catches application-specific exceptions (AppException hierarchy) and returns appropriate HTTP status codes.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                // Known application exception — return structured error
                _logger.LogWarning("Application error: {StatusCode} - {Message}", ex.StatusCode, ex.Message);
                await WriteErrorResponseAsync(context, ex.StatusCode, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access: {Message}", ex.Message);
                await WriteErrorResponseAsync(context, StatusCodes.Status401Unauthorized, ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Bad request: {Message}", ex.Message);
                await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Conflict: {Message}", ex.Message);
                await WriteErrorResponseAsync(context, StatusCodes.Status409Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await WriteErrorResponseAsync(context, StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.");
            }
        }

        private static async Task WriteErrorResponseAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var error = new ErrorResponse(message);
            var json = JsonSerializer.Serialize(error, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
