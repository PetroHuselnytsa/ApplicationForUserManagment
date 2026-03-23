using FluentAssertions;
using Microsoft.AspNetCore.Http;
using TestFirstProject.Middleware;
using Xunit;

namespace TestFirstProject.Tests.Middleware;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoCorrelationId_ShouldGenerate()
    {
        var context = new DefaultHttpContext();
        var nextCalled = false;
        var middleware = new CorrelationIdMiddleware(next: (innerContext) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        context.Items["CorrelationId"].Should().NotBeNull();
        ((string)context.Items["CorrelationId"]!).Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ExistingCorrelationId_ShouldPreserve()
    {
        var context = new DefaultHttpContext();
        var existingId = Guid.NewGuid().ToString();
        context.Request.Headers["X-Correlation-ID"] = existingId;

        var middleware = new CorrelationIdMiddleware(next: (innerContext) => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Items["CorrelationId"].Should().Be(existingId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddCorrelationIdToResponseHeaders()
    {
        var context = new DefaultHttpContext();
        var correlationId = Guid.NewGuid().ToString();
        context.Request.Headers["X-Correlation-ID"] = correlationId;

        var responseStarted = false;
        context.Response.OnStarting(() =>
        {
            responseStarted = true;
            return Task.CompletedTask;
        });

        var middleware = new CorrelationIdMiddleware(next: async (innerContext) =>
        {
            await innerContext.Response.StartAsync();
        });

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(correlationId);
    }

    [Fact]
    public async Task InvokeAsync_GeneratedCorrelationId_ShouldBeValidGuid()
    {
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(next: (innerContext) => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var correlationId = (string)context.Items["CorrelationId"]!;
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_EmptyCorrelationIdHeader_ShouldGenerate()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "";

        var middleware = new CorrelationIdMiddleware(next: (innerContext) => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var correlationId = (string)context.Items["CorrelationId"]!;
        correlationId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_WhitespaceCorrelationIdHeader_ShouldGenerate()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "   ";

        var middleware = new CorrelationIdMiddleware(next: (innerContext) => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var correlationId = (string)context.Items["CorrelationId"]!;
        correlationId.Should().NotBe("   ");
        correlationId.Trim().Should().NotBeEmpty();
    }
}
