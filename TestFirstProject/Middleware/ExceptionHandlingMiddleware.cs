using System.Text.Json;
using TestFirstProject.Exceptions;

namespace TestFirstProject.Middleware
{
    /// <summary>
    /// Global exception handler that converts application exceptions into structured JSON error responses.
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
                _logger.LogWarning(ex, "Application error: {Message}", ex.Message);
                await WriteErrorResponse(context, ex.StatusCode, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
                await WriteErrorResponse(context, 401, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await WriteErrorResponse(context, 500, "An unexpected error occurred.");
            }
        }

        internal static async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var errorResponse = new
            {
                status = statusCode,
                error = statusCode switch
                {
                    400 => "Bad Request",
                    401 => "Unauthorized",
                    403 => "Forbidden",
                    404 => "Not Found",
                    409 => "Conflict",
                    422 => "Unprocessable Entity",
                    429 => "Too Many Requests",
                    _ => "Internal Server Error"
                },
                message
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}
