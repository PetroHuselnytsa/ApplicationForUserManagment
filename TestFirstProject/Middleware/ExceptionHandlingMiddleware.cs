using System.Text.Json;
using TestFirstProject.DTOs.Common;
using TestFirstProject.Exceptions;

namespace TestFirstProject.Middleware
{
    /// <summary>
    /// Global exception handling middleware that catches known exception types
    /// and returns structured JSON error responses with appropriate HTTP status codes.
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
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                ForbiddenException => (StatusCodes.Status403Forbidden, exception.Message),
                BadRequestException => (StatusCodes.Status400BadRequest, exception.Message),
                RateLimitExceededException => (StatusCodes.Status429TooManyRequests, exception.Message),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized access."),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            // Log server errors at error level, client errors at warning level
            if (statusCode >= 500)
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            else
                _logger.LogWarning("Client error ({StatusCode}): {Message}", statusCode, exception.Message);

            var errorResponse = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                Detail = statusCode >= 500 ? null : exception.Message,
                Timestamp = DateTime.UtcNow
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
