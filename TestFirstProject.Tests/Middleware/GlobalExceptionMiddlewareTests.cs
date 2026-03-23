using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TestFirstProject.Middleware;
using TestFirstProject.Services.ErrorTracking;
using TestFirstProject.Services.Telemetry;
using Xunit;

namespace TestFirstProject.Tests.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionMiddleware>> _mockLogger;
    private readonly Mock<IErrorTrackingService> _mockErrorTracking;
    private readonly Mock<ITelemetryService> _mockTelemetry;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;

    public GlobalExceptionMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionMiddleware>>();
        _mockErrorTracking = new Mock<IErrorTrackingService>();
        _mockTelemetry = new Mock<ITelemetryService>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");
    }

    [Fact]
    public async Task InvokeAsync_NoException_ShouldPassThrough()
    {
        var context = CreateHttpContext();
        var nextCalled = false;
        var middleware = CreateMiddleware(next: (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_Exception_ShouldReturn500()
    {
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(next: (ctx) =>
            throw new InvalidOperationException("Test error"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task InvokeAsync_ArgumentException_ShouldReturn400()
    {
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(next: (ctx) =>
            throw new ArgumentException("Invalid argument"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_ShouldReturn404()
    {
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(next: (ctx) =>
            throw new KeyNotFoundException("Not found"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_ShouldReturn401()
    {
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(next: (ctx) =>
            throw new UnauthorizedAccessException("Access denied"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_Exception_ShouldLogError()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Test error");
        var middleware = CreateMiddleware(next: (ctx) => throw exception);

        await middleware.InvokeAsync(context);

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
    public async Task InvokeAsync_Exception_ShouldSendToErrorTracking()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Test error");
        var middleware = CreateMiddleware(next: (ctx) => throw exception);

        await middleware.InvokeAsync(context);

        _mockErrorTracking.Verify(
            x => x.CaptureException(exception, It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_Exception_ShouldTrackInTelemetry()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Test error");
        var middleware = CreateMiddleware(next: (ctx) => throw exception);

        await middleware.InvokeAsync(context);

        _mockTelemetry.Verify(
            x => x.TrackException(exception, It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_Exception_ShouldReturnJsonErrorResponse()
    {
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(next: (ctx) =>
            throw new InvalidOperationException("Test error"));

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        responseBody.Should().NotBeNullOrEmpty();
        context.Response.ContentType.Should().Be("application/json");

        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
        response.TryGetProperty("correlationId", out _).Should().BeTrue();
        response.TryGetProperty("message", out _).Should().BeTrue();
        response.TryGetProperty("statusCode", out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_Exception_InDevelopment_ShouldIncludeDetails()
    {
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(next: (ctx) =>
            throw new InvalidOperationException("Test error"));

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
        response.TryGetProperty("details", out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_Exception_InProduction_ShouldNotIncludeDetails()
    {
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(next: (ctx) =>
            throw new InvalidOperationException("Test error"));

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
        response.TryGetProperty("details", out var details).Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithCorrelationId_ShouldIncludeInResponse()
    {
        var context = CreateHttpContext();
        context.Items["CorrelationId"] = "test-correlation-123";
        var middleware = CreateMiddleware(next: (ctx) =>
            throw new InvalidOperationException("Test error"));

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        responseBody.Should().Contain("test-correlation-123");
    }

    private DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private GlobalExceptionMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new GlobalExceptionMiddleware(
            next,
            _mockLogger.Object,
            _mockErrorTracking.Object,
            _mockTelemetry.Object,
            _mockEnvironment.Object);
    }
}
