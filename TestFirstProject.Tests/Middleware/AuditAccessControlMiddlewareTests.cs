using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using TestFirstProject.Middleware;
using TestFirstProject.Services.Authorization;
using Xunit;

namespace TestFirstProject.Tests.Middleware;

public class AuditAccessControlMiddlewareTests
{
    private readonly Mock<IAuditAuthorizationService> _mockAuthService;

    public AuditAccessControlMiddlewareTests()
    {
        _mockAuthService = new Mock<IAuditAuthorizationService>();
    }

    [Fact]
    public async Task InvokeAsync_NonProtectedPath_ShouldPassThrough()
    {
        var context = CreateHttpContext("/api/users");
        var nextCalled = false;
        var middleware = new AuditAccessControlMiddleware(next: (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, _mockAuthService.Object);

        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Theory]
    [InlineData("/api/audit/search")]
    [InlineData("/api/audit/export")]
    [InlineData("/api/logs/search")]
    [InlineData("/api/logs/analytics/error-rate")]
    public async Task InvokeAsync_ProtectedPath_WithoutAuth_ShouldReturn403(string path)
    {
        var context = CreateHttpContext(path);
        _mockAuthService.Setup(x => x.CanViewAuditLogs(context)).Returns(false);

        var nextCalled = false;
        var middleware = new AuditAccessControlMiddleware(next: (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, _mockAuthService.Object);

        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(403);
    }

    [Theory]
    [InlineData("/api/audit/search")]
    [InlineData("/api/audit/export")]
    [InlineData("/api/logs/search")]
    public async Task InvokeAsync_ProtectedPath_WithAuth_ShouldPassThrough(string path)
    {
        var context = CreateHttpContext(path);
        _mockAuthService.Setup(x => x.CanViewAuditLogs(context)).Returns(true);

        var nextCalled = false;
        var middleware = new AuditAccessControlMiddleware(next: (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, _mockAuthService.Object);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ProtectedPath_WithoutAuth_ShouldReturnJsonError()
    {
        var context = CreateHttpContext("/api/audit/search");
        _mockAuthService.Setup(x => x.CanViewAuditLogs(context)).Returns(false);

        var middleware = new AuditAccessControlMiddleware(next: (ctx) => Task.CompletedTask);
        await middleware.InvokeAsync(context, _mockAuthService.Object);

        context.Response.ContentType.Should().Be("application/json");
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        body.Should().Contain("Forbidden");
    }

    [Fact]
    public async Task InvokeAsync_CaseInsensitivePath_ShouldBeProtected()
    {
        var context = CreateHttpContext("/API/AUDIT/SEARCH");
        _mockAuthService.Setup(x => x.CanViewAuditLogs(context)).Returns(false);

        var middleware = new AuditAccessControlMiddleware(next: (ctx) => Task.CompletedTask);
        await middleware.InvokeAsync(context, _mockAuthService.Object);

        context.Response.StatusCode.Should().Be(403);
    }

    private static DefaultHttpContext CreateHttpContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }
}
