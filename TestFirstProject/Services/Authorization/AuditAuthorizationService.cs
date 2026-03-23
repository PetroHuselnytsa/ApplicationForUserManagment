namespace TestFirstProject.Services.Authorization;

/// <summary>
/// Service for authorizing access to audit logs using header-based authentication.
/// </summary>
public class AuditAuthorizationService : IAuditAuthorizationService
{
    private readonly string _adminKey;
    private const string AdminKeyHeader = "X-Admin-Key";

    /// <summary>
    /// Initializes a new instance of the AuditAuthorizationService.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    public AuditAuthorizationService(IConfiguration configuration)
    {
        _adminKey = configuration["Audit:AdminKey"] ?? "default-admin-key-change-me";
    }

    /// <inheritdoc />
    public bool CanViewAuditLogs(HttpContext context)
    {
        return ValidateAdminKey(context);
    }

    /// <inheritdoc />
    public bool CanExportAuditLogs(HttpContext context)
    {
        return ValidateAdminKey(context);
    }

    /// <inheritdoc />
    public bool CanViewAllUsers(HttpContext context)
    {
        return ValidateAdminKey(context);
    }

    private bool ValidateAdminKey(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(AdminKeyHeader, out var key))
        {
            return false;
        }

        return !string.IsNullOrEmpty(key) && key == _adminKey;
    }
}
