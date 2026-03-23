using System.Net;
using System.Text.Json;
using TestFirstProject.Services.ErrorTracking;
using TestFirstProject.Services.Logging;
using TestFirstProject.Services.Telemetry;

namespace TestFirstProject.Middleware;

/// <summary>
/// Middleware for handling unhandled exceptions globally.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IErrorTrackingService _errorTrackingService;
    private readonly ITelemetryService _telemetryService;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the GlobalExceptionMiddleware.
    /// </summary>
    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IErrorTrackingService errorTrackingService,
        ITelemetryService telemetryService,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _errorTrackingService = errorTrackingService;
        _telemetryService = telemetryService;
        _environment = environment;
    }

    /// <summary>
    /// Processes the HTTP request and catches any unhandled exceptions.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex).ConfigureAwait(false);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";

        _logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
            correlationId,
            context.Request.Path,
            context.Request.Method);

        var errorContext = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Path"] = context.Request.Path.ToString(),
            ["Method"] = context.Request.Method,
            ["QueryString"] = context.Request.QueryString.ToString(),
            ["UserAgent"] = context.Request.Headers.UserAgent.ToString(),
            ["ClientIP"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        };

        _errorTrackingService.CaptureException(exception, errorContext);

        _telemetryService.TrackException(exception, new Dictionary<string, string>
        {
            ["CorrelationId"] = correlationId,
            ["Path"] = context.Request.Path.ToString(),
            ["Method"] = context.Request.Method
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = GetStatusCode(exception);

        var response = new ErrorResponse
        {
            CorrelationId = correlationId,
            Message = GetSafeErrorMessage(exception),
            StatusCode = context.Response.StatusCode
        };

        if (_environment.IsDevelopment())
        {
            response.Details = exception.ToString();
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions)).ConfigureAwait(false);
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            InvalidOperationException => (int)HttpStatusCode.Conflict,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    private string GetSafeErrorMessage(Exception exception)
    {
        if (_environment.IsDevelopment())
        {
            return exception.Message;
        }

        return exception switch
        {
            ArgumentException => "Invalid request parameters.",
            KeyNotFoundException => "The requested resource was not found.",
            UnauthorizedAccessException => "Access denied.",
            InvalidOperationException => "The requested operation is not valid.",
            _ => "An unexpected error occurred. Please try again later."
        };
    }

    private class ErrorResponse
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string? Details { get; set; }
    }
}
