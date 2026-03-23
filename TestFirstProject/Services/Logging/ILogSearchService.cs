namespace TestFirstProject.Services.Logging;

/// <summary>
/// Interface for searching application logs.
/// </summary>
public interface ILogSearchService
{
    /// <summary>
    /// Searches logs based on the specified criteria.
    /// </summary>
    /// <param name="criteria">The search criteria.</param>
    /// <returns>A paged result of log entries.</returns>
    Task<LogSearchResult> SearchLogsAsync(LogSearchCriteria criteria);
}

/// <summary>
/// Criteria for searching logs.
/// </summary>
public class LogSearchCriteria
{
    /// <summary>
    /// Filter by log level (DEBUG, INFO, WARN, ERROR, CRITICAL).
    /// </summary>
    public string? Level { get; set; }

    /// <summary>
    /// Filter logs from this timestamp.
    /// </summary>
    public DateTime? FromTimestamp { get; set; }

    /// <summary>
    /// Filter logs to this timestamp.
    /// </summary>
    public DateTime? ToTimestamp { get; set; }

    /// <summary>
    /// Filter by correlation ID.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Filter by user ID.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Search text within message content.
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Page number (1-based). Default is 1.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page. Default is 100.
    /// </summary>
    public int PageSize { get; set; } = 100;
}

/// <summary>
/// Represents a log entry.
/// </summary>
public class LogEntry
{
    /// <summary>
    /// Timestamp of the log entry.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Log level.
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Log message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Correlation ID for request tracking.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// User ID if available.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Additional properties.
    /// </summary>
    public Dictionary<string, object>? Properties { get; set; }
}

/// <summary>
/// Result of a log search operation.
/// </summary>
public class LogSearchResult
{
    /// <summary>
    /// The log entries in the current page.
    /// </summary>
    public IEnumerable<LogEntry> Items { get; set; } = Enumerable.Empty<LogEntry>();

    /// <summary>
    /// Total count of matching entries.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
