namespace TestFirstProject.Services.Logging;

/// <summary>
/// Service for log analytics and visualization.
/// </summary>
public class LogAnalyticsService : ILogAnalyticsService
{
    private readonly ILogSearchService _logSearchService;
    private readonly ILogger<LogAnalyticsService> _logger;

    /// <summary>
    /// Initializes a new instance of the LogAnalyticsService.
    /// </summary>
    public LogAnalyticsService(ILogSearchService logSearchService, ILogger<LogAnalyticsService> logger)
    {
        _logSearchService = logSearchService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TimeSeriesPoint>> GetErrorRateAsync(DateTime from, DateTime to, string interval)
    {
        var criteria = new LogSearchCriteria
        {
            FromTimestamp = from,
            ToTimestamp = to,
            PageSize = int.MaxValue
        };

        var logs = await _logSearchService.SearchLogsAsync(criteria).ConfigureAwait(false);
        var errorLogs = logs.Items.Where(l =>
            l.Level.Equals("Error", StringComparison.OrdinalIgnoreCase) ||
            l.Level.Equals("Critical", StringComparison.OrdinalIgnoreCase) ||
            l.Level.Equals("Fatal", StringComparison.OrdinalIgnoreCase));

        return AggregateByInterval(errorLogs, interval, from, to);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TimeSeriesPoint>> GetRequestVolumeAsync(DateTime from, DateTime to, string? endpoint, string interval)
    {
        var criteria = new LogSearchCriteria
        {
            FromTimestamp = from,
            ToTimestamp = to,
            SearchText = endpoint,
            PageSize = int.MaxValue
        };

        var logs = await _logSearchService.SearchLogsAsync(criteria).ConfigureAwait(false);
        var requestLogs = logs.Items.Where(l => l.Message.Contains("HTTP Request", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(endpoint))
        {
            requestLogs = requestLogs.Where(l => l.Message.Contains(endpoint, StringComparison.OrdinalIgnoreCase));
        }

        return AggregateByInterval(requestLogs, interval, from, to);
    }

    /// <inheritdoc />
    public async Task<ResponseTimeStats> GetResponseTimesAsync(DateTime from, DateTime to, string? endpoint)
    {
        var criteria = new LogSearchCriteria
        {
            FromTimestamp = from,
            ToTimestamp = to,
            SearchText = "Duration",
            PageSize = int.MaxValue
        };

        var logs = await _logSearchService.SearchLogsAsync(criteria).ConfigureAwait(false);
        var responseTimes = new List<double>();

        foreach (var log in logs.Items)
        {
            if (!string.IsNullOrEmpty(endpoint) && !log.Message.Contains(endpoint, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (log.Properties?.TryGetValue("DurationMs", out var duration) == true)
            {
                if (double.TryParse(duration?.ToString(), out var ms))
                {
                    responseTimes.Add(ms);
                }
            }
        }

        if (responseTimes.Count == 0)
        {
            return new ResponseTimeStats();
        }

        responseTimes.Sort();

        return new ResponseTimeStats
        {
            Average = responseTimes.Average(),
            P50 = GetPercentile(responseTimes, 50),
            P90 = GetPercentile(responseTimes, 90),
            P99 = GetPercentile(responseTimes, 99),
            Min = responseTimes.Min(),
            Max = responseTimes.Max(),
            TotalRequests = responseTimes.Count
        };
    }

    private static IEnumerable<TimeSeriesPoint> AggregateByInterval(IEnumerable<LogEntry> logs, string interval, DateTime from, DateTime to)
    {
        var grouped = logs.GroupBy(l => GetIntervalKey(l.Timestamp, interval))
            .ToDictionary(g => g.Key, g => g.Count());

        var points = new List<TimeSeriesPoint>();
        var current = from;

        while (current <= to)
        {
            var key = GetIntervalKey(current, interval);
            points.Add(new TimeSeriesPoint
            {
                Timestamp = current,
                Value = grouped.GetValueOrDefault(key, 0),
                Label = key
            });

            current = interval.ToLowerInvariant() switch
            {
                "hour" => current.AddHours(1),
                "day" => current.AddDays(1),
                "week" => current.AddDays(7),
                _ => current.AddHours(1)
            };
        }

        return points;
    }

    private static string GetIntervalKey(DateTime timestamp, string interval)
    {
        return interval.ToLowerInvariant() switch
        {
            "hour" => timestamp.ToString("yyyy-MM-dd HH:00"),
            "day" => timestamp.ToString("yyyy-MM-dd"),
            "week" => $"{timestamp.Year}-W{GetWeekOfYear(timestamp):D2}",
            _ => timestamp.ToString("yyyy-MM-dd HH:00")
        };
    }

    private static int GetWeekOfYear(DateTime date)
    {
        var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    private static double GetPercentile(List<double> sortedValues, int percentile)
    {
        if (sortedValues.Count == 0)
        {
            return 0;
        }

        var index = (percentile / 100.0) * (sortedValues.Count - 1);
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);

        if (lower == upper)
        {
            return sortedValues[lower];
        }

        return sortedValues[lower] + (index - lower) * (sortedValues[upper] - sortedValues[lower]);
    }
}
