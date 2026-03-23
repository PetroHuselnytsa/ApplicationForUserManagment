using TestFirstProject.Contexts;
using TestFirstProject.Services.Audit;
using TestFirstProject.Services.Audit.Models;

namespace TestFirstProject.Extensions;

/// <summary>
/// Extension methods for configuring audit logging services.
/// </summary>
public static class AuditExtensions
{
    /// <summary>
    /// Adds audit logging services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAuditLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuditLoggingConfiguration>(configuration.GetSection("AuditLogging"));

        services.AddDbContext<AuditContext>();
        services.AddSingleton<IChecksumService, ChecksumService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}
