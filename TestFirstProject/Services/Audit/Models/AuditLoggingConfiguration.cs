namespace TestFirstProject.Services.Audit.Models;

/// <summary>
/// Configuration settings for audit logging.
/// </summary>
public class AuditLoggingConfiguration
{
    /// <summary>
    /// Whether audit logging is enabled. Default is true.
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>
    /// Number of days to retain audit logs. Default is 365.
    /// </summary>
    public int RetentionDays { get; set; } = 365;

    /// <summary>
    /// Secret key used for generating checksums. Should be kept secure.
    /// </summary>
    public string? ChecksumSecretKey { get; set; }
}
