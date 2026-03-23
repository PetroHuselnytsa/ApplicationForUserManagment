using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TestFirstProject.Middleware;
using TestFirstProject.Services.Logging;
using TestFirstProject.Services.Logging.Models;
using Xunit;

namespace TestFirstProject.Tests.Middleware;

public class RequestLoggingMiddlewareTests
{
    private readonly Mock<ILogger<RequestLoggingMiddleware>> _mockLogger;
    private readonly Mock<IPiiMaskingService> _mockPiiService;
    private readonly RequestLoggingOptions _options;

    public RequestLoggingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<RequestLoggingMiddleware>>();
        _mockPiiService = new Mock<IPiiMaskingService>();
        _mockPiiService.Setup(x => x.MaskSensitiveData(It.IsAny<string>()))
            .Returns<string>(s => s);

        _options = new RequestLoggingOptions
        {
            LogRequestBody = true,
            LogResponseBody = true,
            MaxBodyLogLength = 4096,
            ExcludedPaths = new List<string> { "/health", "/metrics" },
            SensitiveHeaders = new List<string> { "Authorization", "X-Api-Key" }
        };
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogRequestDetails()
    {
        var context = CreateHttpContext("GET", "/api/users");
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP Request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogResponseDetails()
    {
        var context = CreateHttpContext("GET", "/api/users");
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP Response")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldMaskSensitiveData()
    {
        var context = CreateHttpContext("POST", "/api/users");
        var requestBody = """{"password":"secret123"}""";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
        context.Request.ContentLength = requestBody.Length;

        _mockPiiService.Setup(x => x.MaskSensitiveData(It.IsAny<string>()))
            .Returns("""{"password":"***"}""");

        var middleware = CreateMiddleware();
        await middleware.InvokeAsync(context);

        _mockPiiService.Verify(x => x.MaskSensitiveData(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/metrics")]
    [InlineData("/health/live")]
    public async Task InvokeAsync_ExcludedPath_ShouldNotLog(string path)
    {
        var context = CreateHttpContext("GET", path);
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP Request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ShouldRespectMaxBodyLength()
    {
        var context = CreateHttpContext("POST", "/api/users");
        var longBody = new string('x', 10000);
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(longBody));
        context.Request.ContentLength = longBody.Length;

        _options.MaxBodyLogLength = 100;
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        _mockPiiService.Verify(
            x => x.MaskSensitiveData(It.Is<string>(s => s.Length <= 150)),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_ShouldMaskSensitiveHeaders()
    {
        var context = CreateHttpContext("GET", "/api/users");
        context.Request.Headers["Authorization"] = "Bearer secret-token";
        context.Request.Headers["X-Api-Key"] = "api-key-123";

        var middleware = CreateMiddleware();
        await middleware.InvokeAsync(context);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => !v.ToString()!.Contains("secret-token")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_WithCorrelationId_ShouldIncludeInLog()
    {
        var context = CreateHttpContext("GET", "/api/users");
        context.Items["CorrelationId"] = "test-correlation-id";

        var middleware = CreateMiddleware();
        await middleware.InvokeAsync(context);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("test-correlation-id")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private DefaultHttpContext CreateHttpContext(string method, string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.Body = new MemoryStream();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private RequestLoggingMiddleware CreateMiddleware()
    {
        return new RequestLoggingMiddleware(
            next: (ctx) =>
            {
                ctx.Response.StatusCode = 200;
                return Task.CompletedTask;
            },
            _mockLogger.Object,
            _mockPiiService.Object,
            Options.Create(_options));
    }
}
