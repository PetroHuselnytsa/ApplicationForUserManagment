using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace TestFirstProject.Services.Logging;

/// <summary>
/// Centralized logging service implementation using Serilog for structured JSON logging.
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly AsyncLocal<string> _correlationId = new();

    /// <summary>
    /// Initializes a new instance of the LoggingService.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="httpContextAccessor">HTTP context accessor for request-scoped data.</param>
    public LoggingService(ILogger<LoggingService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public void LogDebug(string message, params object[] args)
    {
        using (PushCorrelationContext())
        {
            _logger.LogDebug(message, args);
        }
    }

    /// <inheritdoc />
    public void LogInformation(string message, params object[] args)
    {
        using (PushCorrelationContext())
        {
            _logger.LogInformation(message, args);
        }
    }

    /// <inheritdoc />
    public void LogWarning(string message, params object[] args)
    {
        using (PushCorrelationContext())
        {
            _logger.LogWarning(message, args);
        }
    }

    /// <inheritdoc />
    public void LogError(string message, Exception? exception = null, params object[] args)
    {
        using (PushCorrelationContext())
        {
            if (exception != null)
            {
                _logger.LogError(exception, message, args);
            }
            else
            {
                _logger.LogError(message, args);
            }
        }
    }

    /// <inheritdoc />
    public void LogCritical(string message, Exception? exception = null, params object[] args)
    {
        using (PushCorrelationContext())
        {
            if (exception != null)
            {
                _logger.LogCritical(exception, message, args);
            }
            else
            {
                _logger.LogCritical(message, args);
            }
        }
    }

    /// <inheritdoc />
    public void LogWithContext(LogLevel level, string message, Dictionary<string, object>? context = null)
    {
        using (PushCorrelationContext())
        {
            if (context != null)
            {
                using var _ = PushContextProperties(context);
                _logger.Log(level, message);
            }
            else
            {
                _logger.Log(level, message);
            }
        }
    }

    /// <inheritdoc />
    public string GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items.TryGetValue("CorrelationId", out var correlationId) == true && correlationId is string id)
        {
            return id;
        }

        return _correlationId.Value ?? string.Empty;
    }

    /// <inheritdoc />
    public void SetCorrelationId(string correlationId)
    {
        _correlationId.Value = correlationId;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Items["CorrelationId"] = correlationId;
        }
    }

    private IDisposable PushCorrelationContext()
    {
        var correlationId = GetCorrelationId();
        return LogContext.PushProperty("CorrelationId", correlationId);
    }

    private IDisposable PushContextProperties(Dictionary<string, object> context)
    {
        var disposables = new List<IDisposable>();
        foreach (var kvp in context)
        {
            disposables.Add(LogContext.PushProperty(kvp.Key, kvp.Value));
        }
        return new CompositeDisposable(disposables);
    }

    private class CompositeDisposable : IDisposable
    {
        private readonly IEnumerable<IDisposable> _disposables;

        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
