namespace TestFirstProject.Services.Authorization;

/// <summary>
/// Interface for audit log authorization service.
/// </summary>
public interface IAuditAuthorizationService
{
    /// <summary>
    /// Checks if the current user can view audit logs.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>True if authorized.</returns>
    bool CanViewAuditLogs(HttpContext context);

    /// <summary>
    /// Checks if the current user can export audit logs.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>True if authorized.</returns>
    bool CanExportAuditLogs(HttpContext context);

    /// <summary>
    /// Checks if the current user can view all users' audit logs.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>True if authorized.</returns>
    bool CanViewAllUsers(HttpContext context);
}
