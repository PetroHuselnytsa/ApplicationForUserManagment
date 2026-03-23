using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TestFirstProject.Services.Logging;
using Xunit;

namespace TestFirstProject.Tests.Services;

public class LoggingServiceTests
{
    private readonly Mock<ILogger<LoggingService>> _mockLogger;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly ILoggingService _service;

    public LoggingServiceTests()
    {
        _mockLogger = new Mock<ILogger<LoggingService>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
        _service = new LoggingService(_mockLogger.Object, _mockHttpContextAccessor.Object);
    }

    [Fact]
    public void LogDebug_ShouldLogWithCorrectLevel()
    {
        _service.LogDebug("Test debug message");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test debug message")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogInformation_ShouldLogWithCorrectLevel()
    {
        _service.LogInformation("Test info message");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test info message")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWarning_ShouldLogWithCorrectLevel()
    {
        _service.LogWarning("Test warning message");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test warning message")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogError_ShouldIncludeException()
    {
        var exception = new InvalidOperationException("Test error");

        _service.LogError("Error occurred", exception);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogError_WithoutException_ShouldLog()
    {
        _service.LogError("Error without exception");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCritical_ShouldIncludeException()
    {
        var exception = new Exception("Critical error");

        _service.LogCritical("Critical error occurred", exception);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetCorrelationId_AfterSet_ShouldReturnSetValue()
    {
        var correlationId = Guid.NewGuid().ToString();

        _service.SetCorrelationId(correlationId);
        var result = _service.GetCorrelationId();

        result.Should().Be(correlationId);
    }

    [Fact]
    public void GetCorrelationId_WithHttpContext_ShouldReturnFromContext()
    {
        var correlationId = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        context.Items["CorrelationId"] = correlationId;
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);

        var service = new LoggingService(_mockLogger.Object, _mockHttpContextAccessor.Object);
        var result = service.GetCorrelationId();

        result.Should().Be(correlationId);
    }

    [Fact]
    public void SetCorrelationId_ShouldStoreInHttpContext()
    {
        var correlationId = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);

        var service = new LoggingService(_mockLogger.Object, _mockHttpContextAccessor.Object);
        service.SetCorrelationId(correlationId);

        context.Items["CorrelationId"].Should().Be(correlationId);
    }

    [Fact]
    public void LogWithContext_ShouldLogMessage()
    {
        var context = new Dictionary<string, object> { { "UserId", "123" }, { "Action", "Login" } };

        _service.LogWithContext(LogLevel.Information, "User action");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
