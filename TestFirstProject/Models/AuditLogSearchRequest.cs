namespace TestFirstProject.Models;

/// <summary>
/// Request model for searching and filtering audit logs.
/// </summary>
public class AuditLogSearchRequest
{
    /// <summary>
    /// Filter by user ID.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Filter by entity type.
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Filter by entity ID.
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Filter by action type.
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Filter by start date (inclusive).
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter by end date (inclusive).
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Filter by correlation ID.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Page number (1-based). Default is 1.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page. Default is 50.
    /// </summary>
    public int PageSize { get; set; } = 50;
}
