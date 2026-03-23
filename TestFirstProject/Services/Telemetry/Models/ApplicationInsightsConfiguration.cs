namespace TestFirstProject.Services.Telemetry.Models;

/// <summary>
/// Configuration settings for Azure Application Insights.
/// </summary>
public class ApplicationInsightsConfiguration
{
    /// <summary>
    /// The Application Insights connection string.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Whether to enable adaptive sampling. Default is true.
    /// </summary>
    public bool EnableAdaptiveSampling { get; set; } = true;

    /// <summary>
    /// Whether to enable dependency tracking. Default is true.
    /// </summary>
    public bool EnableDependencyTracking { get; set; } = true;

    /// <summary>
    /// Whether to enable performance counters. Default is true.
    /// </summary>
    public bool EnablePerformanceCounters { get; set; } = true;
}
