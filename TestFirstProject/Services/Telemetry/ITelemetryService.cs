namespace TestFirstProject.Services.Telemetry;

/// <summary>
/// Interface for telemetry operations (Application Insights integration).
/// </summary>
public interface ITelemetryService
{
    /// <summary>
    /// Tracks a custom event.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="properties">Optional properties to associate with the event.</param>
    void TrackEvent(string eventName, Dictionary<string, string>? properties = null);

    /// <summary>
    /// Tracks a custom metric.
    /// </summary>
    /// <param name="name">The name of the metric.</param>
    /// <param name="value">The value of the metric.</param>
    /// <param name="properties">Optional properties to associate with the metric.</param>
    void TrackMetric(string name, double value, Dictionary<string, string>? properties = null);

    /// <summary>
    /// Tracks an exception.
    /// </summary>
    /// <param name="exception">The exception to track.</param>
    /// <param name="properties">Optional properties to associate with the exception.</param>
    void TrackException(Exception exception, Dictionary<string, string>? properties = null);

    /// <summary>
    /// Tracks a dependency call.
    /// </summary>
    /// <param name="dependencyType">The type of dependency (e.g., "SQL", "HTTP").</param>
    /// <param name="name">The name of the dependency.</param>
    /// <param name="data">The command/data used.</param>
    /// <param name="startTime">The start time of the call.</param>
    /// <param name="duration">The duration of the call.</param>
    /// <param name="success">Whether the call succeeded.</param>
    void TrackDependency(string dependencyType, string name, string data, DateTimeOffset startTime, TimeSpan duration, bool success);

    /// <summary>
    /// Tracks a request.
    /// </summary>
    /// <param name="name">The name of the request.</param>
    /// <param name="startTime">The start time of the request.</param>
    /// <param name="duration">The duration of the request.</param>
    /// <param name="responseCode">The response code.</param>
    /// <param name="success">Whether the request succeeded.</param>
    void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success);

    /// <summary>
    /// Flushes the telemetry buffer.
    /// </summary>
    void Flush();
}
