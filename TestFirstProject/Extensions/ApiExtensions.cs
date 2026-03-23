using TestFirstProject.Middleware;
using TestFirstProject.Services.Authorization;
using TestFirstProject.Services.Logging;

namespace TestFirstProject.Extensions;

/// <summary>
/// Extension methods for configuring API services.
/// </summary>
public static class ApiExtensions
{
    /// <summary>
    /// Adds API services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ILogSearchService, LogSearchService>();
        services.AddScoped<ILogAnalyticsService, LogAnalyticsService>();
        services.AddScoped<IAuditAuthorizationService, AuditAuthorizationService>();

        return services;
    }

    /// <summary>
    /// Adds audit access control middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseAuditAccessControl(this WebApplication app)
    {
        app.UseMiddleware<AuditAccessControlMiddleware>();
        return app;
    }
}
