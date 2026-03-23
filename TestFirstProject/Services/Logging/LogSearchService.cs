using System.Text.Json;
using Microsoft.Extensions.Options;
using TestFirstProject.Services.Logging.Models;

namespace TestFirstProject.Services.Logging;

/// <summary>
/// Service for searching application log files.
/// </summary>
public class LogSearchService : ILogSearchService
{
    private readonly LoggingConfiguration _config;
    private readonly ILogger<LogSearchService> _logger;

    /// <summary>
    /// Initializes a new instance of the LogSearchService.
    /// </summary>
    public LogSearchService(IOptions<LoggingConfiguration> options, ILogger<LogSearchService> logger)
    {
        _config = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<LogSearchResult> SearchLogsAsync(LogSearchCriteria criteria)
    {
        var entries = new List<LogEntry>();

        try
        {
            var logDirectory = Path.GetDirectoryName(_config.LogFilePath) ?? "logs";
            if (!Directory.Exists(logDirectory))
            {
                return new LogSearchResult
                {
                    Items = entries,
                    TotalCount = 0,
                    Page = criteria.Page,
                    PageSize = criteria.PageSize
                };
            }

            var logFiles = Directory.GetFiles(logDirectory, "*.log")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .ToList();

            foreach (var file in logFiles)
            {
                await ReadLogFileAsync(file, entries, criteria).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading log files");
        }

        var filteredEntries = ApplyFilters(entries, criteria);
        var totalCount = filteredEntries.Count();

        var pagedEntries = filteredEntries
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToList();

        return new LogSearchResult
        {
            Items = pagedEntries,
            TotalCount = totalCount,
            Page = criteria.Page,
            PageSize = criteria.PageSize
        };
    }

    private async Task ReadLogFileAsync(string filePath, List<LogEntry> entries, LogSearchCriteria criteria)
    {
        try
        {
            using var reader = new StreamReader(filePath);
            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var entry = ParseLogLine(line);
                if (entry != null)
                {
                    entries.Add(entry);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading log file: {FilePath}", filePath);
        }
    }

    private static LogEntry? ParseLogLine(string line)
    {
        try
        {
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;

            var entry = new LogEntry
            {
                Timestamp = root.TryGetProperty("@t", out var timestamp)
                    ? DateTime.Parse(timestamp.GetString() ?? DateTime.UtcNow.ToString("O"))
                    : DateTime.UtcNow,
                Level = root.TryGetProperty("@l", out var level)
                    ? level.GetString() ?? "Information"
                    : "Information",
                Message = root.TryGetProperty("@m", out var message)
                    ? message.GetString() ?? string.Empty
                    : string.Empty,
                Properties = new Dictionary<string, object>()
            };

            if (root.TryGetProperty("CorrelationId", out var correlationId))
            {
                entry.CorrelationId = correlationId.GetString();
            }

            if (root.TryGetProperty("UserId", out var userId))
            {
                entry.UserId = userId.GetString();
            }

            foreach (var prop in root.EnumerateObject())
            {
                if (!prop.Name.StartsWith("@") && prop.Name != "CorrelationId" && prop.Name != "UserId")
                {
                    entry.Properties[prop.Name] = prop.Value.ToString();
                }
            }

            return entry;
        }
        catch
        {
            return null;
        }
    }

    private static IEnumerable<LogEntry> ApplyFilters(List<LogEntry> entries, LogSearchCriteria criteria)
    {
        var query = entries.AsEnumerable();

        if (!string.IsNullOrEmpty(criteria.Level))
        {
            query = query.Where(e => e.Level.Equals(criteria.Level, StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.FromTimestamp.HasValue)
        {
            query = query.Where(e => e.Timestamp >= criteria.FromTimestamp.Value);
        }

        if (criteria.ToTimestamp.HasValue)
        {
            query = query.Where(e => e.Timestamp <= criteria.ToTimestamp.Value);
        }

        if (!string.IsNullOrEmpty(criteria.CorrelationId))
        {
            query = query.Where(e => e.CorrelationId == criteria.CorrelationId);
        }

        if (!string.IsNullOrEmpty(criteria.UserId))
        {
            query = query.Where(e => e.UserId == criteria.UserId);
        }

        if (!string.IsNullOrEmpty(criteria.SearchText))
        {
            query = query.Where(e => e.Message.Contains(criteria.SearchText, StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderByDescending(e => e.Timestamp);
    }
}
