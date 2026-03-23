namespace TestFirstProject.Models;

/// <summary>
/// Represents an audit log entry for tracking system changes and user actions.
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Unique identifier for the audit log entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Timestamp when the action occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Identifier of the user who performed the action.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the user who performed the action.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// The type of action performed (CREATE, UPDATE, DELETE, READ).
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The type of entity being acted upon (e.g., "Person").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The identifier of the entity being acted upon.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// JSON snapshot of the entity values before the change.
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// JSON snapshot of the entity values after the change.
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Additional context data as JSON.
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// IP address of the client making the request.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User agent string of the client.
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Correlation ID for request tracing across services.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// The request path that triggered this action.
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// The HTTP method of the request.
    /// </summary>
    public string? RequestMethod { get; set; }

    /// <summary>
    /// The HTTP response status code.
    /// </summary>
    public int? ResponseStatusCode { get; set; }

    /// <summary>
    /// The environment where this action occurred.
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// SHA256 checksum for tamper detection.
    /// </summary>
    public string Checksum { get; set; } = string.Empty;
}
