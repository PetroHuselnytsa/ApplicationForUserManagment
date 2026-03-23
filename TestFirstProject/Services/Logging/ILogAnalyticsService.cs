namespace TestFirstProject.Services.Logging;

/// <summary>
/// Interface for log analytics and visualization services.
/// </summary>
public interface ILogAnalyticsService
{
    /// <summary>
    /// Gets error rate over time.
    /// </summary>
    /// <param name="from">Start date.</param>
    /// <param name="to">End date.</param>
    /// <param name="interval">Time interval (hour, day, week).</param>
    /// <returns>Time series data for error rate.</returns>
    Task<IEnumerable<TimeSeriesPoint>> GetErrorRateAsync(DateTime from, DateTime to, string interval);

    /// <summary>
    /// Gets request volume over time.
    /// </summary>
    /// <param name="from">Start date.</param>
    /// <param name="to">End date.</param>
    /// <param name="endpoint">Optional endpoint filter.</param>
    /// <param name="interval">Time interval (hour, day, week).</param>
    /// <returns>Time series data for request volume.</returns>
    Task<IEnumerable<TimeSeriesPoint>> GetRequestVolumeAsync(DateTime from, DateTime to, string? endpoint, string interval);

    /// <summary>
    /// Gets response time statistics.
    /// </summary>
    /// <param name="from">Start date.</param>
    /// <param name="to">End date.</param>
    /// <param name="endpoint">Optional endpoint filter.</param>
    /// <returns>Response time statistics.</returns>
    Task<ResponseTimeStats> GetResponseTimesAsync(DateTime from, DateTime to, string? endpoint);
}

/// <summary>
/// Represents a point in a time series.
/// </summary>
public class TimeSeriesPoint
{
    /// <summary>
    /// Timestamp for the data point.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Value at this timestamp.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Optional label for the data point.
    /// </summary>
    public string? Label { get; set; }
}

/// <summary>
/// Response time statistics.
/// </summary>
public class ResponseTimeStats
{
    /// <summary>
    /// Average response time in milliseconds.
    /// </summary>
    public double Average { get; set; }

    /// <summary>
    /// 50th percentile (median) response time.
    /// </summary>
    public double P50 { get; set; }

    /// <summary>
    /// 90th percentile response time.
    /// </summary>
    public double P90 { get; set; }

    /// <summary>
    /// 99th percentile response time.
    /// </summary>
    public double P99 { get; set; }

    /// <summary>
    /// Minimum response time.
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// Maximum response time.
    /// </summary>
    public double Max { get; set; }

    /// <summary>
    /// Total number of requests.
    /// </summary>
    public int TotalRequests { get; set; }
}
