using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;
using TestFirstProject.Services.Telemetry.Models;

namespace TestFirstProject.Services.Telemetry;

/// <summary>
/// Service for Azure Application Insights telemetry integration.
/// </summary>
public class ApplicationInsightsService : ITelemetryService
{
    private readonly TelemetryClient? _telemetryClient;
    private readonly ILogger<ApplicationInsightsService> _logger;
    private readonly bool _isEnabled;

    /// <summary>
    /// Initializes a new instance of the ApplicationInsightsService.
    /// </summary>
    public ApplicationInsightsService(
        IOptions<ApplicationInsightsConfiguration> options,
        ILogger<ApplicationInsightsService> logger,
        TelemetryClient? telemetryClient = null)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
        _isEnabled = !string.IsNullOrEmpty(options.Value.ConnectionString);

        if (!_isEnabled)
        {
            _logger.LogInformation("Application Insights is not configured. Telemetry will be disabled.");
        }
    }

    /// <inheritdoc />
    public void TrackEvent(string eventName, Dictionary<string, string>? properties = null)
    {
        if (!_isEnabled || _telemetryClient == null)
        {
            return;
        }

        try
        {
            _telemetryClient.TrackEvent(eventName, properties);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track event: {EventName}", eventName);
        }
    }

    /// <inheritdoc />
    public void TrackMetric(string name, double value, Dictionary<string, string>? properties = null)
    {
        if (!_isEnabled || _telemetryClient == null)
        {
            return;
        }

        try
        {
            var metric = new MetricTelemetry(name, value);
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    metric.Properties[prop.Key] = prop.Value;
                }
            }
            _telemetryClient.TrackMetric(metric);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track metric: {MetricName}", name);
        }
    }

    /// <inheritdoc />
    public void TrackException(Exception exception, Dictionary<string, string>? properties = null)
    {
        if (!_isEnabled || _telemetryClient == null)
        {
            return;
        }

        try
        {
            _telemetryClient.TrackException(exception, properties);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track exception");
        }
    }

    /// <inheritdoc />
    public void TrackDependency(string dependencyType, string name, string data, DateTimeOffset startTime, TimeSpan duration, bool success)
    {
        if (!_isEnabled || _telemetryClient == null)
        {
            return;
        }

        try
        {
            var dependency = new DependencyTelemetry(dependencyType, name, name, data, startTime, duration, success ? "200" : "500", success);
            _telemetryClient.TrackDependency(dependency);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track dependency: {DependencyName}", name);
        }
    }

    /// <inheritdoc />
    public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
    {
        if (!_isEnabled || _telemetryClient == null)
        {
            return;
        }

        try
        {
            var request = new RequestTelemetry(name, startTime, duration, responseCode, success);
            _telemetryClient.TrackRequest(request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track request: {RequestName}", name);
        }
    }

    /// <inheritdoc />
    public void Flush()
    {
        try
        {
            _telemetryClient?.Flush();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to flush telemetry");
        }
    }
}
