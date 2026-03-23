using Microsoft.Extensions.Options;
using Sentry;
using TestFirstProject.Services.ErrorTracking.Models;

namespace TestFirstProject.Services.ErrorTracking;

/// <summary>
/// Service for Sentry error tracking integration.
/// </summary>
public class SentryService : IErrorTrackingService
{
    private readonly ILogger<SentryService> _logger;
    private readonly bool _isEnabled;

    /// <summary>
    /// Initializes a new instance of the SentryService.
    /// </summary>
    public SentryService(
        IOptions<SentryConfiguration> options,
        ILogger<SentryService> logger)
    {
        _logger = logger;
        _isEnabled = !string.IsNullOrEmpty(options.Value.Dsn);

        if (!_isEnabled)
        {
            _logger.LogInformation("Sentry is not configured. Error tracking will be disabled.");
        }
    }

    /// <inheritdoc />
    public string? CaptureException(Exception exception, Dictionary<string, object>? context = null)
    {
        if (!_isEnabled)
        {
            return null;
        }

        try
        {
            SentrySdk.ConfigureScope(scope =>
            {
                if (context != null)
                {
                    foreach (var item in context)
                    {
                        scope.SetExtra(item.Key, item.Value);
                    }
                }
            });

            var eventId = SentrySdk.CaptureException(exception);
            return eventId.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to capture exception in Sentry");
            return null;
        }
    }

    /// <inheritdoc />
    public string? CaptureMessage(string message, ErrorSeverityLevel level = ErrorSeverityLevel.Info)
    {
        if (!_isEnabled)
        {
            return null;
        }

        try
        {
            var sentryLevel = MapSeverityLevel(level);
            var eventId = SentrySdk.CaptureMessage(message, sentryLevel);
            return eventId.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to capture message in Sentry");
            return null;
        }
    }

    /// <inheritdoc />
    public void AddBreadcrumb(string message, string category, Dictionary<string, string>? data = null)
    {
        if (!_isEnabled)
        {
            return;
        }

        try
        {
            SentrySdk.AddBreadcrumb(message, category, data: data);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add breadcrumb in Sentry");
        }
    }

    /// <inheritdoc />
    public void SetUser(string userId, string? email = null, string? username = null)
    {
        if (!_isEnabled)
        {
            return;
        }

        try
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.User = new SentryUser
                {
                    Id = userId,
                    Email = email,
                    Username = username
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set user in Sentry");
        }
    }

    /// <inheritdoc />
    public void SetTag(string key, string value)
    {
        if (!_isEnabled)
        {
            return;
        }

        try
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag(key, value);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set tag in Sentry");
        }
    }

    private static SentryLevel MapSeverityLevel(ErrorSeverityLevel level)
    {
        return level switch
        {
            ErrorSeverityLevel.Debug => SentryLevel.Debug,
            ErrorSeverityLevel.Info => SentryLevel.Info,
            ErrorSeverityLevel.Warning => SentryLevel.Warning,
            ErrorSeverityLevel.Error => SentryLevel.Error,
            ErrorSeverityLevel.Fatal => SentryLevel.Fatal,
            _ => SentryLevel.Info
        };
    }
}
