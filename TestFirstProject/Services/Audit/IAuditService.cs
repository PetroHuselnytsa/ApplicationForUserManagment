using TestFirstProject.Models;

namespace TestFirstProject.Services.Audit;

/// <summary>
/// Interface for audit logging operations.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit entry with the provided details.
    /// </summary>
    /// <param name="entry">The audit log entry details.</param>
    Task LogAsync(AuditLogEntry entry);

    /// <summary>
    /// Logs a CREATE action for an entity.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="userId">The user ID performing the action.</param>
    /// <param name="userName">The user name performing the action.</param>
    /// <param name="entity">The created entity.</param>
    /// <param name="additionalData">Optional additional context data.</param>
    Task LogCreateAsync<T>(string userId, string userName, T entity, string? additionalData = null) where T : class;

    /// <summary>
    /// Logs an UPDATE action for an entity.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="userId">The user ID performing the action.</param>
    /// <param name="userName">The user name performing the action.</param>
    /// <param name="oldEntity">The entity state before the update.</param>
    /// <param name="newEntity">The entity state after the update.</param>
    /// <param name="additionalData">Optional additional context data.</param>
    Task LogUpdateAsync<T>(string userId, string userName, T oldEntity, T newEntity, string? additionalData = null) where T : class;

    /// <summary>
    /// Logs a DELETE action for an entity.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="userId">The user ID performing the action.</param>
    /// <param name="userName">The user name performing the action.</param>
    /// <param name="entity">The deleted entity.</param>
    /// <param name="additionalData">Optional additional context data.</param>
    Task LogDeleteAsync<T>(string userId, string userName, T entity, string? additionalData = null) where T : class;

    /// <summary>
    /// Logs a READ action for an entity.
    /// </summary>
    /// <param name="userId">The user ID performing the action.</param>
    /// <param name="userName">The user name performing the action.</param>
    /// <param name="entityType">The type of entity being read.</param>
    /// <param name="entityId">The ID of the entity being read.</param>
    /// <param name="additionalData">Optional additional context data.</param>
    Task LogReadAsync(string userId, string userName, string entityType, string entityId, string? additionalData = null);

    /// <summary>
    /// Searches audit logs with the specified filters.
    /// </summary>
    /// <param name="request">The search request with filters.</param>
    /// <returns>A paged result of audit logs.</returns>
    Task<PagedResult<AuditLog>> SearchAsync(AuditLogSearchRequest request);

    /// <summary>
    /// Gets the history of changes for a specific entity.
    /// </summary>
    /// <param name="entityType">The type of entity.</param>
    /// <param name="entityId">The ID of the entity.</param>
    /// <returns>A collection of audit logs for the entity.</returns>
    Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(string entityType, string entityId);

    /// <summary>
    /// Exports audit logs matching the search criteria.
    /// </summary>
    /// <param name="request">The search request with filters.</param>
    /// <param name="format">The export format.</param>
    /// <returns>A stream containing the exported data.</returns>
    Task<Stream> ExportAsync(AuditLogSearchRequest request, ExportFormat format);

    /// <summary>
    /// Verifies the integrity of an audit log entry.
    /// </summary>
    /// <param name="auditLogId">The ID of the audit log to verify.</param>
    /// <returns>True if the audit log has not been tampered with.</returns>
    Task<bool> VerifyIntegrityAsync(Guid auditLogId);
}

/// <summary>
/// Export format options for audit logs.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// Comma-separated values format.
    /// </summary>
    Csv,

    /// <summary>
    /// JSON format.
    /// </summary>
    Json
}

/// <summary>
/// Represents an audit log entry to be created.
/// </summary>
public class AuditLogEntry
{
    /// <summary>
    /// The user ID performing the action.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The user name performing the action.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// The action being performed.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The type of entity being acted upon.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the entity being acted upon.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// The entity values before the change.
    /// </summary>
    public object? OldValues { get; set; }

    /// <summary>
    /// The entity values after the change.
    /// </summary>
    public object? NewValues { get; set; }

    /// <summary>
    /// Additional context data.
    /// </summary>
    public object? AdditionalData { get; set; }
}

/// <summary>
/// Represents a paged result set.
/// </summary>
/// <typeparam name="T">The type of items in the result.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// The items in the current page.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// The total count of items matching the query.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
