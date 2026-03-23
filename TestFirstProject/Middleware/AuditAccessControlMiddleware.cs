using System.Text.Json;
using TestFirstProject.Services.Authorization;

namespace TestFirstProject.Middleware;

/// <summary>
/// Middleware for controlling access to audit and log endpoints.
/// </summary>
public class AuditAccessControlMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string[] ProtectedPaths = { "/api/audit", "/api/logs" };

    /// <summary>
    /// Initializes a new instance of the AuditAccessControlMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public AuditAccessControlMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Processes the HTTP request and enforces access control.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="authService">The audit authorization service.</param>
    public async Task InvokeAsync(HttpContext context, IAuditAuthorizationService authService)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        if (IsProtectedPath(path))
        {
            if (!authService.CanViewAuditLogs(context))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Forbidden",
                    message = "Access to audit logs requires admin authorization. Please provide a valid X-Admin-Key header."
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response)).ConfigureAwait(false);
                return;
            }
        }

        await _next(context).ConfigureAwait(false);
    }

    private static bool IsProtectedPath(string path)
    {
        return ProtectedPaths.Any(protectedPath =>
            path.StartsWith(protectedPath, StringComparison.OrdinalIgnoreCase));
    }
}
