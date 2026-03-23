namespace TestFirstProject.Services.ErrorTracking.Models;

/// <summary>
/// Configuration settings for Sentry error tracking.
/// </summary>
public class SentryConfiguration
{
    /// <summary>
    /// The Sentry DSN (Data Source Name).
    /// </summary>
    public string? Dsn { get; set; }

    /// <summary>
    /// The environment name (e.g., "Development", "Production").
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// The sample rate for errors (0.0 to 1.0). Default is 1.0 (100%).
    /// </summary>
    public double SampleRate { get; set; } = 1.0;

    /// <summary>
    /// The sample rate for traces (0.0 to 1.0). Default is 0.5 (50%).
    /// </summary>
    public double TracesSampleRate { get; set; } = 0.5;

    /// <summary>
    /// Whether to attach stack traces. Default is true.
    /// </summary>
    public bool AttachStacktrace { get; set; } = true;

    /// <summary>
    /// Whether to send default PII. Default is false for privacy.
    /// </summary>
    public bool SendDefaultPii { get; set; } = false;
}
