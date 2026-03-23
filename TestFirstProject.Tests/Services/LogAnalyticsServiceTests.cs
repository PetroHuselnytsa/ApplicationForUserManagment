using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TestFirstProject.Services.Logging;
using Xunit;

namespace TestFirstProject.Tests.Services;

public class LogAnalyticsServiceTests
{
    private readonly Mock<ILogSearchService> _mockLogSearchService;
    private readonly Mock<ILogger<LogAnalyticsService>> _mockLogger;
    private readonly ILogAnalyticsService _service;

    public LogAnalyticsServiceTests()
    {
        _mockLogSearchService = new Mock<ILogSearchService>();
        _mockLogger = new Mock<ILogger<LogAnalyticsService>>();

        _mockLogSearchService.Setup(x => x.SearchLogsAsync(It.IsAny<LogSearchCriteria>()))
            .ReturnsAsync(new LogSearchResult
            {
                Items = new List<LogEntry>(),
                TotalCount = 0,
                Page = 1,
                PageSize = int.MaxValue
            });

        _service = new LogAnalyticsService(_mockLogSearchService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetErrorRateAsync_EmptyLogs_ReturnsEmptyTimeSeries()
    {
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        var result = await _service.GetErrorRateAsync(from, to, "day");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetErrorRateAsync_WithErrors_AggregatesByInterval()
    {
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        _mockLogSearchService.Setup(x => x.SearchLogsAsync(It.IsAny<LogSearchCriteria>()))
            .ReturnsAsync(new LogSearchResult
            {
                Items = new List<LogEntry>
                {
                    new LogEntry { Timestamp = from.AddDays(1), Level = "Error", Message = "Test error" },
                    new LogEntry { Timestamp = from.AddDays(1), Level = "Error", Message = "Another error" },
                    new LogEntry { Timestamp = from.AddDays(2), Level = "Error", Message = "Third error" }
                },
                TotalCount = 3,
                Page = 1,
                PageSize = int.MaxValue
            });

        var result = await _service.GetErrorRateAsync(from, to, "day");

        result.Should().NotBeNull();
        var resultList = result.ToList();
        resultList.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("hour")]
    [InlineData("day")]
    [InlineData("week")]
    public async Task GetErrorRateAsync_DifferentIntervals_ReturnsData(string interval)
    {
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        var result = await _service.GetErrorRateAsync(from, to, interval);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRequestVolumeAsync_EmptyLogs_ReturnsEmptyTimeSeries()
    {
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        var result = await _service.GetRequestVolumeAsync(from, to, null, "day");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRequestVolumeAsync_WithEndpointFilter_FiltersCorrectly()
    {
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        var result = await _service.GetRequestVolumeAsync(from, to, "/api/users", "day");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetResponseTimesAsync_EmptyLogs_ReturnsZeroStats()
    {
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        var result = await _service.GetResponseTimesAsync(from, to, null);

        result.Should().NotBeNull();
        result.TotalRequests.Should().Be(0);
    }

    [Fact]
    public async Task GetResponseTimesAsync_WithData_CalculatesPercentiles()
    {
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        _mockLogSearchService.Setup(x => x.SearchLogsAsync(It.IsAny<LogSearchCriteria>()))
            .ReturnsAsync(new LogSearchResult
            {
                Items = new List<LogEntry>
                {
                    new LogEntry
                    {
                        Timestamp = from.AddDays(1),
                        Level = "Information",
                        Message = "HTTP Response: Duration: 100ms",
                        Properties = new Dictionary<string, object> { { "DurationMs", "100" } }
                    },
                    new LogEntry
                    {
                        Timestamp = from.AddDays(1),
                        Level = "Information",
                        Message = "HTTP Response: Duration: 200ms",
                        Properties = new Dictionary<string, object> { { "DurationMs", "200" } }
                    },
                    new LogEntry
                    {
                        Timestamp = from.AddDays(2),
                        Level = "Information",
                        Message = "HTTP Response: Duration: 300ms",
                        Properties = new Dictionary<string, object> { { "DurationMs", "300" } }
                    }
                },
                TotalCount = 3,
                Page = 1,
                PageSize = int.MaxValue
            });

        var result = await _service.GetResponseTimesAsync(from, to, null);

        result.Should().NotBeNull();
        result.TotalRequests.Should().Be(3);
        result.Average.Should().Be(200);
        result.Min.Should().Be(100);
        result.Max.Should().Be(300);
    }

    [Fact]
    public async Task GetResponseTimesAsync_WithEndpointFilter_FiltersCorrectly()
    {
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        var result = await _service.GetResponseTimesAsync(from, to, "/api/users");

        result.Should().NotBeNull();
    }
}
