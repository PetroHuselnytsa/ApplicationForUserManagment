namespace TestFirstProject.Middleware;

/// <summary>
/// Middleware for managing correlation IDs to track requests across services.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    /// <summary>
    /// Initializes a new instance of the CorrelationIdMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Processes the HTTP request to add or propagate correlation ID.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.Items["CorrelationId"] = correlationId;

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers.Append(CorrelationIdHeader, correlationId);
            }
            return Task.CompletedTask;
        });

        await _next(context).ConfigureAwait(false);
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existingCorrelationId)
            && !string.IsNullOrWhiteSpace(existingCorrelationId))
        {
            return existingCorrelationId.ToString();
        }

        return Guid.NewGuid().ToString("D");
    }
}
