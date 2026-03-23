using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;
using TestFirstProject.Services.Logging;
using TestFirstProject.Services.Logging.Models;

namespace TestFirstProject.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with PII masking.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly IPiiMaskingService _piiMaskingService;
    private readonly RequestLoggingOptions _options;

    /// <summary>
    /// Initializes a new instance of the RequestLoggingMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="piiMaskingService">PII masking service for sensitive data.</param>
    /// <param name="options">Request logging options.</param>
    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        IPiiMaskingService piiMaskingService,
        IOptions<RequestLoggingOptions> options)
    {
        _next = next;
        _logger = logger;
        _piiMaskingService = piiMaskingService;
        _options = options.Value;
    }

    /// <summary>
    /// Processes the HTTP request and logs request/response details.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkipLogging(context.Request.Path))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";

        var requestBody = string.Empty;
        if (_options.LogRequestBody && context.Request.ContentLength > 0)
        {
            requestBody = await ReadRequestBodyAsync(context.Request).ConfigureAwait(false);
        }

        LogRequest(context, correlationId, requestBody);

        var originalResponseBody = context.Response.Body;
        using var responseBodyStream = new MemoryStream();

        if (_options.LogResponseBody)
        {
            context.Response.Body = responseBodyStream;
        }

        try
        {
            await _next(context).ConfigureAwait(false);
        }
        finally
        {
            stopwatch.Stop();

            var responseBody = string.Empty;
            if (_options.LogResponseBody)
            {
                responseBody = await ReadResponseBodyAsync(responseBodyStream, originalResponseBody).ConfigureAwait(false);
            }

            LogResponse(context, correlationId, stopwatch.ElapsedMilliseconds, responseBody);
        }
    }

    private bool ShouldSkipLogging(PathString path)
    {
        return _options.ExcludedPaths.Any(excludedPath =>
            path.StartsWithSegments(excludedPath, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 4096,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync().ConfigureAwait(false);
        request.Body.Position = 0;

        return TruncateAndMask(body);
    }

    private async Task<string> ReadResponseBodyAsync(MemoryStream responseBodyStream, Stream originalBody)
    {
        responseBodyStream.Position = 0;
        var body = await new StreamReader(responseBodyStream).ReadToEndAsync().ConfigureAwait(false);

        responseBodyStream.Position = 0;
        await responseBodyStream.CopyToAsync(originalBody).ConfigureAwait(false);

        return TruncateAndMask(body);
    }

    private string TruncateAndMask(string body)
    {
        if (string.IsNullOrEmpty(body))
        {
            return body;
        }

        var truncated = body.Length > _options.MaxBodyLogLength
            ? body.Substring(0, _options.MaxBodyLogLength) + "...[TRUNCATED]"
            : body;

        return _piiMaskingService.MaskSensitiveData(truncated);
    }

    private void LogRequest(HttpContext context, string correlationId, string requestBody)
    {
        var request = context.Request;
        var headers = GetMaskedHeaders(request.Headers);

        _logger.LogInformation(
            "HTTP Request: {Method} {Path}{QueryString} | CorrelationId: {CorrelationId} | ClientIP: {ClientIP} | UserAgent: {UserAgent} | Headers: {Headers} | Body: {Body}",
            request.Method,
            request.Path,
            _piiMaskingService.MaskSensitiveData(request.QueryString.ToString()),
            correlationId,
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            request.Headers.UserAgent.ToString(),
            headers,
            requestBody);
    }

    private void LogResponse(HttpContext context, string correlationId, long elapsedMs, string responseBody)
    {
        var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error
            : context.Response.StatusCode >= 400 ? LogLevel.Warning
            : LogLevel.Information;

        _logger.Log(
            logLevel,
            "HTTP Response: {StatusCode} | CorrelationId: {CorrelationId} | Duration: {DurationMs}ms | Body: {Body}",
            context.Response.StatusCode,
            correlationId,
            elapsedMs,
            responseBody);
    }

    private Dictionary<string, string> GetMaskedHeaders(IHeaderDictionary headers)
    {
        var result = new Dictionary<string, string>();

        foreach (var header in headers)
        {
            if (_options.SensitiveHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
            {
                result[header.Key] = "[REDACTED]";
            }
            else
            {
                result[header.Key] = _piiMaskingService.MaskSensitiveData(header.Value.ToString());
            }
        }

        return result;
    }
}
