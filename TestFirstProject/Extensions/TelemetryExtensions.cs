using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using TestFirstProject.Middleware;
using TestFirstProject.Services.ErrorTracking;
using TestFirstProject.Services.ErrorTracking.Models;
using TestFirstProject.Services.Telemetry;
using TestFirstProject.Services.Telemetry.Models;

namespace TestFirstProject.Extensions;

/// <summary>
/// Extension methods for configuring telemetry and error tracking services.
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>
    /// Adds telemetry and error tracking services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTelemetryServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApplicationInsightsConfiguration>(configuration.GetSection("ApplicationInsights"));
        services.Configure<SentryConfiguration>(configuration.GetSection("Sentry"));
        services.Configure<PerformanceMonitoringOptions>(configuration.GetSection("PerformanceMonitoring"));

        var appInsightsConfig = configuration.GetSection("ApplicationInsights").Get<ApplicationInsightsConfiguration>();
        if (!string.IsNullOrEmpty(appInsightsConfig?.ConnectionString))
        {
            services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                ConnectionString = appInsightsConfig.ConnectionString,
                EnableAdaptiveSampling = appInsightsConfig.EnableAdaptiveSampling,
                EnableDependencyTrackingTelemetryModule = appInsightsConfig.EnableDependencyTracking,
                EnablePerformanceCounterCollectionModule = appInsightsConfig.EnablePerformanceCounters
            });
        }

        services.AddSingleton<ITelemetryService, ApplicationInsightsService>();
        services.AddSingleton<IErrorTrackingService, SentryService>();

        return services;
    }

    /// <summary>
    /// Configures Sentry for error tracking in the web application builder.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <returns>The web application builder for chaining.</returns>
    public static WebApplicationBuilder ConfigureSentry(this WebApplicationBuilder builder)
    {
        var sentryConfig = builder.Configuration.GetSection("Sentry").Get<SentryConfiguration>();

        if (!string.IsNullOrEmpty(sentryConfig?.Dsn))
        {
            builder.WebHost.UseSentry(options =>
            {
                options.Dsn = sentryConfig.Dsn;
                options.Environment = sentryConfig.Environment;
                options.SampleRate = (float)sentryConfig.SampleRate;
                options.TracesSampleRate = sentryConfig.TracesSampleRate;
                options.AttachStacktrace = sentryConfig.AttachStacktrace;
                options.SendDefaultPii = sentryConfig.SendDefaultPii;
            });
        }

        return builder;
    }

    /// <summary>
    /// Adds error tracking and performance monitoring middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseErrorTracking(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<PerformanceMonitoringMiddleware>();
        return app;
    }
}
