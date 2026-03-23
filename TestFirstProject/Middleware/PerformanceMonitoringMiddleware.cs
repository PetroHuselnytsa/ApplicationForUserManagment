using System.Diagnostics;
using Microsoft.Extensions.Options;
using TestFirstProject.Services.Telemetry;

namespace TestFirstProject.Middleware;

/// <summary>
/// Middleware for monitoring request performance and logging slow requests.
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly ITelemetryService _telemetryService;
    private readonly PerformanceMonitoringOptions _options;

    /// <summary>
    /// Initializes a new instance of the PerformanceMonitoringMiddleware.
    /// </summary>
    public PerformanceMonitoringMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMonitoringMiddleware> logger,
        ITelemetryService telemetryService,
        IOptions<PerformanceMonitoringOptions> options)
    {
        _next = next;
        _logger = logger;
        _telemetryService = telemetryService;
        _options = options.Value;
    }

    /// <summary>
    /// Processes the HTTP request and monitors its performance.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context).ConfigureAwait(false);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            RecordMetrics(context, startTime, stopwatch.Elapsed, elapsedMs);

            if (elapsedMs > _options.SlowRequestThresholdMs)
            {
                LogSlowRequest(context, correlationId, elapsedMs);
            }
        }
    }

    private void RecordMetrics(HttpContext context, DateTimeOffset startTime, TimeSpan duration, long elapsedMs)
    {
        var requestName = $"{context.Request.Method} {context.Request.Path}";
        var success = context.Response.StatusCode < 400;

        _telemetryService.TrackRequest(
            requestName,
            startTime,
            duration,
            context.Response.StatusCode.ToString(),
            success);

        _telemetryService.TrackMetric(
            "RequestDuration",
            elapsedMs,
            new Dictionary<string, string>
            {
                ["Path"] = context.Request.Path.ToString(),
                ["Method"] = context.Request.Method,
                ["StatusCode"] = context.Response.StatusCode.ToString()
            });

        _telemetryService.TrackEvent(
            "RequestCompleted",
            new Dictionary<string, string>
            {
                ["Path"] = context.Request.Path.ToString(),
                ["Method"] = context.Request.Method,
                ["StatusCode"] = context.Response.StatusCode.ToString(),
                ["DurationMs"] = elapsedMs.ToString()
            });
    }

    private void LogSlowRequest(HttpContext context, string correlationId, long elapsedMs)
    {
        _logger.LogWarning(
            "Slow request detected: {Method} {Path} took {ElapsedMs}ms (threshold: {ThresholdMs}ms). CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            elapsedMs,
            _options.SlowRequestThresholdMs,
            correlationId);

        _telemetryService.TrackEvent(
            "SlowRequest",
            new Dictionary<string, string>
            {
                ["Path"] = context.Request.Path.ToString(),
                ["Method"] = context.Request.Method,
                ["DurationMs"] = elapsedMs.ToString(),
                ["CorrelationId"] = correlationId
            });
    }
}

/// <summary>
/// Configuration options for performance monitoring.
/// </summary>
public class PerformanceMonitoringOptions
{
    /// <summary>
    /// Threshold in milliseconds for logging slow requests. Default is 1000ms.
    /// </summary>
    public int SlowRequestThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Whether to enable slow query logging. Default is true.
    /// </summary>
    public bool EnableSlowQueryLogging { get; set; } = true;

    /// <summary>
    /// Threshold in milliseconds for logging slow database queries. Default is 500ms.
    /// </summary>
    public int SlowQueryThresholdMs { get; set; } = 500;
}
