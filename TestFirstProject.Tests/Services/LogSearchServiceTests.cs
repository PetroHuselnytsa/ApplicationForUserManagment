using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TestFirstProject.Services.Logging;
using TestFirstProject.Services.Logging.Models;
using Xunit;

namespace TestFirstProject.Tests.Services;

public class LogSearchServiceTests
{
    private readonly Mock<ILogger<LogSearchService>> _mockLogger;
    private readonly LoggingConfiguration _config;
    private readonly ILogSearchService _service;

    public LogSearchServiceTests()
    {
        _mockLogger = new Mock<ILogger<LogSearchService>>();
        _config = new LoggingConfiguration
        {
            LogFilePath = "logs/app-.log"
        };
        _service = new LogSearchService(Options.Create(_config), _mockLogger.Object);
    }

    [Fact]
    public async Task SearchLogsAsync_EmptyDirectory_ReturnsEmptyResult()
    {
        var criteria = new LogSearchCriteria
        {
            Page = 1,
            PageSize = 10
        };

        var result = await _service.SearchLogsAsync(criteria);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchLogsAsync_WithFilters_AppliesFilters()
    {
        var criteria = new LogSearchCriteria
        {
            Level = "Error",
            FromTimestamp = DateTime.UtcNow.AddDays(-1),
            ToTimestamp = DateTime.UtcNow.AddDays(1),
            Page = 1,
            PageSize = 10
        };

        var result = await _service.SearchLogsAsync(criteria);

        result.Should().NotBeNull();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task SearchLogsAsync_Pagination_ReturnsCorrectPage()
    {
        var criteria = new LogSearchCriteria
        {
            Page = 2,
            PageSize = 5
        };

        var result = await _service.SearchLogsAsync(criteria);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task SearchLogsAsync_SearchText_FiltersByMessage()
    {
        var criteria = new LogSearchCriteria
        {
            SearchText = "error",
            Page = 1,
            PageSize = 100
        };

        var result = await _service.SearchLogsAsync(criteria);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchLogsAsync_CorrelationIdFilter_AppliesCorrectly()
    {
        var correlationId = Guid.NewGuid().ToString();
        var criteria = new LogSearchCriteria
        {
            CorrelationId = correlationId,
            Page = 1,
            PageSize = 100
        };

        var result = await _service.SearchLogsAsync(criteria);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchLogsAsync_UserIdFilter_AppliesCorrectly()
    {
        var criteria = new LogSearchCriteria
        {
            UserId = "test-user",
            Page = 1,
            PageSize = 100
        };

        var result = await _service.SearchLogsAsync(criteria);

        result.Should().NotBeNull();
    }
}
