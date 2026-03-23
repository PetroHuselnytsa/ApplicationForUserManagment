namespace TestFirstProject.Services.Logging.Models;

/// <summary>
/// Configuration settings for the logging infrastructure.
/// </summary>
public class LoggingConfiguration
{
    /// <summary>
    /// The minimum log level to capture. Default is "Information".
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// Whether to enable console logging. Default is true.
    /// </summary>
    public bool EnableConsole { get; set; } = true;

    /// <summary>
    /// Whether to enable file logging. Default is true.
    /// </summary>
    public bool EnableFile { get; set; } = true;

    /// <summary>
    /// The path for log files. Uses rolling file pattern. Default is "logs/app-.log".
    /// </summary>
    public string LogFilePath { get; set; } = "logs/app-.log";

    /// <summary>
    /// Number of days to retain log files. Default is 30.
    /// </summary>
    public int RetentionDays { get; set; } = 30;

    /// <summary>
    /// Application Insights connection string for cloud logging. Optional.
    /// </summary>
    public string? ApplicationInsightsConnectionString { get; set; }

    /// <summary>
    /// Sentry DSN for error tracking. Optional.
    /// </summary>
    public string? SentryDsn { get; set; }
}
