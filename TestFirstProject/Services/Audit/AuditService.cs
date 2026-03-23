using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TestFirstProject.Contexts;
using TestFirstProject.Models;
using TestFirstProject.Services.Audit.Models;
using TestFirstProject.Services.Logging;

namespace TestFirstProject.Services.Audit;

/// <summary>
/// Service for managing audit log operations.
/// </summary>
public class AuditService : IAuditService
{
    private readonly AuditContext _auditContext;
    private readonly ILoggingService _loggingService;
    private readonly IChecksumService _checksumService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuditLoggingConfiguration _config;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the AuditService.
    /// </summary>
    public AuditService(
        AuditContext auditContext,
        ILoggingService loggingService,
        IChecksumService checksumService,
        IHttpContextAccessor httpContextAccessor,
        IOptions<AuditLoggingConfiguration> options,
        IWebHostEnvironment environment)
    {
        _auditContext = auditContext;
        _loggingService = loggingService;
        _checksumService = checksumService;
        _httpContextAccessor = httpContextAccessor;
        _config = options.Value;
        _environment = environment;
    }

    /// <inheritdoc />
    public async Task LogAsync(AuditLogEntry entry)
    {
        if (!_config.EnableAuditLogging)
        {
            return;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            UserId = entry.UserId,
            UserName = entry.UserName,
            Action = entry.Action,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            OldValues = entry.OldValues != null ? JsonSerializer.Serialize(entry.OldValues) : null,
            NewValues = entry.NewValues != null ? JsonSerializer.Serialize(entry.NewValues) : null,
            AdditionalData = entry.AdditionalData != null ? JsonSerializer.Serialize(entry.AdditionalData) : null,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty,
            CorrelationId = _loggingService.GetCorrelationId(),
            RequestPath = httpContext?.Request.Path.ToString(),
            RequestMethod = httpContext?.Request.Method,
            Environment = _environment.EnvironmentName
        };

        auditLog.Checksum = _checksumService.GenerateChecksum(auditLog);

        try
        {
            _auditContext.AuditLogs.Add(auditLog);
            await _auditContext.SaveChangesAsync().ConfigureAwait(false);

            _loggingService.LogInformation(
                "Audit log created: {Action} on {EntityType}/{EntityId} by {UserId}",
                entry.Action, entry.EntityType, entry.EntityId, entry.UserId);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(
                "Failed to create audit log: {Message}",
                ex,
                ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task LogCreateAsync<T>(string userId, string userName, T entity, string? additionalData = null) where T : class
    {
        var entry = new AuditLogEntry
        {
            UserId = userId,
            UserName = userName,
            Action = "CREATE",
            EntityType = typeof(T).Name,
            EntityId = GetEntityId(entity),
            NewValues = entity,
            AdditionalData = additionalData
        };

        await LogAsync(entry).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task LogUpdateAsync<T>(string userId, string userName, T oldEntity, T newEntity, string? additionalData = null) where T : class
    {
        var entry = new AuditLogEntry
        {
            UserId = userId,
            UserName = userName,
            Action = "UPDATE",
            EntityType = typeof(T).Name,
            EntityId = GetEntityId(newEntity),
            OldValues = oldEntity,
            NewValues = newEntity,
            AdditionalData = additionalData
        };

        await LogAsync(entry).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task LogDeleteAsync<T>(string userId, string userName, T entity, string? additionalData = null) where T : class
    {
        var entry = new AuditLogEntry
        {
            UserId = userId,
            UserName = userName,
            Action = "DELETE",
            EntityType = typeof(T).Name,
            EntityId = GetEntityId(entity),
            OldValues = entity,
            AdditionalData = additionalData
        };

        await LogAsync(entry).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task LogReadAsync(string userId, string userName, string entityType, string entityId, string? additionalData = null)
    {
        var entry = new AuditLogEntry
        {
            UserId = userId,
            UserName = userName,
            Action = "READ",
            EntityType = entityType,
            EntityId = entityId,
            AdditionalData = additionalData
        };

        await LogAsync(entry).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PagedResult<AuditLog>> SearchAsync(AuditLogSearchRequest request)
    {
        var query = _auditContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrEmpty(request.UserId))
        {
            query = query.Where(a => a.UserId == request.UserId);
        }

        if (!string.IsNullOrEmpty(request.EntityType))
        {
            query = query.Where(a => a.EntityType == request.EntityType);
        }

        if (!string.IsNullOrEmpty(request.EntityId))
        {
            query = query.Where(a => a.EntityId == request.EntityId);
        }

        if (!string.IsNullOrEmpty(request.Action))
        {
            query = query.Where(a => a.Action == request.Action);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= request.ToDate.Value);
        }

        if (!string.IsNullOrEmpty(request.CorrelationId))
        {
            query = query.Where(a => a.CorrelationId == request.CorrelationId);
        }

        var totalCount = await query.CountAsync().ConfigureAwait(false);

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync()
            .ConfigureAwait(false);

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(string entityType, string entityId)
    {
        return await _auditContext.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Stream> ExportAsync(AuditLogSearchRequest request, ExportFormat format)
    {
        var unlimitedRequest = new AuditLogSearchRequest
        {
            UserId = request.UserId,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Action = request.Action,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            CorrelationId = request.CorrelationId,
            Page = 1,
            PageSize = int.MaxValue
        };

        var result = await SearchAsync(unlimitedRequest).ConfigureAwait(false);
        var stream = new MemoryStream();

        if (format == ExportFormat.Json)
        {
            await JsonSerializer.SerializeAsync(stream, result.Items).ConfigureAwait(false);
        }
        else
        {
            await WriteCsvAsync(stream, result.Items).ConfigureAwait(false);
        }

        stream.Position = 0;
        return stream;
    }

    /// <inheritdoc />
    public async Task<bool> VerifyIntegrityAsync(Guid auditLogId)
    {
        var auditLog = await _auditContext.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == auditLogId)
            .ConfigureAwait(false);

        if (auditLog == null)
        {
            return false;
        }

        return _checksumService.VerifyChecksum(auditLog);
    }

    private static string GetEntityId<T>(T entity) where T : class
    {
        var idProperty = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idProperty != null)
        {
            var value = idProperty.GetValue(entity);
            return value?.ToString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static async Task WriteCsvAsync(Stream stream, IEnumerable<AuditLog> items)
    {
        using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

        await writer.WriteLineAsync("Id,Timestamp,UserId,UserName,Action,EntityType,EntityId,CorrelationId,Environment")
            .ConfigureAwait(false);

        foreach (var item in items)
        {
            var line = $"\"{item.Id}\",\"{item.Timestamp:O}\",\"{EscapeCsv(item.UserId)}\",\"{EscapeCsv(item.UserName)}\",\"{item.Action}\",\"{item.EntityType}\",\"{EscapeCsv(item.EntityId)}\",\"{item.CorrelationId}\",\"{item.Environment}\"";
            await writer.WriteLineAsync(line).ConfigureAwait(false);
        }

        await writer.FlushAsync().ConfigureAwait(false);
    }

    private static string EscapeCsv(string value)
    {
        return value?.Replace("\"", "\"\"") ?? string.Empty;
    }
}
