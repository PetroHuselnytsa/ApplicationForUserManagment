using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using TestFirstProject.Middleware;
using TestFirstProject.Services.Logging;
using TestFirstProject.Services.Logging.Models;

namespace TestFirstProject.Extensions;

/// <summary>
/// Extension methods for configuring logging infrastructure.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adds logging infrastructure services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLoggingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LoggingConfiguration>(configuration.GetSection("LoggingConfiguration"));
        services.Configure<RequestLoggingOptions>(configuration.GetSection("RequestLogging"));
        services.Configure<PiiMaskingConfiguration>(configuration.GetSection("PiiMasking"));

        services.AddHttpContextAccessor();
        services.AddSingleton<IPiiMaskingService, PiiMaskingService>();
        services.AddScoped<ILoggingService, LoggingService>();

        return services;
    }

    /// <summary>
    /// Configures Serilog with the application's logging settings.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <returns>The web application builder for chaining.</returns>
    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        var loggingConfig = builder.Configuration.GetSection("LoggingConfiguration").Get<LoggingConfiguration>()
            ?? new LoggingConfiguration();

        var logLevel = ParseLogLevel(loggingConfig.MinimumLevel);

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentName();

        if (loggingConfig.EnableConsole)
        {
            loggerConfig.WriteTo.Console(new CompactJsonFormatter());
        }

        if (loggingConfig.EnableFile)
        {
            loggerConfig.WriteTo.File(
                new CompactJsonFormatter(),
                loggingConfig.LogFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: loggingConfig.RetentionDays,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1));
        }

        Log.Logger = loggerConfig.CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }

    /// <summary>
    /// Adds logging middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseLoggingMiddleware(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }

    private static LogEventLevel ParseLogLevel(string level)
    {
        return level.ToLowerInvariant() switch
        {
            "verbose" or "trace" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" or "info" => LogEventLevel.Information,
            "warning" or "warn" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" or "critical" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}
