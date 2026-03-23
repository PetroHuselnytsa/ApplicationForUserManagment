using Microsoft.AspNetCore.Mvc;
using TestFirstProject.Models;
using TestFirstProject.Services.Audit;

namespace TestFirstProject.Endpoints;

/// <summary>
/// Audit log API endpoints.
/// </summary>
public static class AuditEndpoints
{
    /// <summary>
    /// Maps audit log endpoints to the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapAuditEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/audit");

        group.MapGet("/search", SearchAuditLogs)
            .WithName("SearchAuditLogs")
            .WithOpenApi();

        group.MapGet("/entity/{entityType}/{entityId}/history", GetEntityHistory)
            .WithName("GetEntityHistory")
            .WithOpenApi();

        group.MapGet("/export", ExportAuditLogs)
            .WithName("ExportAuditLogs")
            .WithOpenApi();

        group.MapGet("/{id:guid}/verify", VerifyIntegrity)
            .WithName("VerifyAuditLogIntegrity")
            .WithOpenApi();
    }

    private static async Task<IResult> SearchAuditLogs(
        [FromQuery] string? userId,
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] string? action,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? correlationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromServices] IAuditService auditService)
    {
        var request = new AuditLogSearchRequest
        {
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            FromDate = fromDate,
            ToDate = toDate,
            CorrelationId = correlationId,
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 100)
        };

        var result = await auditService.SearchAsync(request).ConfigureAwait(false);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetEntityHistory(
        string entityType,
        string entityId,
        [FromServices] IAuditService auditService)
    {
        var history = await auditService.GetEntityHistoryAsync(entityType, entityId).ConfigureAwait(false);
        return Results.Ok(history);
    }

    private static async Task<IResult> ExportAuditLogs(
        [FromQuery] string format,
        [FromQuery] string? userId,
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] string? action,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? correlationId,
        [FromServices] IAuditService auditService,
        HttpResponse response)
    {
        var request = new AuditLogSearchRequest
        {
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            FromDate = fromDate,
            ToDate = toDate,
            CorrelationId = correlationId,
            Page = 1,
            PageSize = int.MaxValue
        };

        var exportFormat = format?.ToLowerInvariant() == "csv" ? ExportFormat.Csv : ExportFormat.Json;
        var stream = await auditService.ExportAsync(request, exportFormat).ConfigureAwait(false);

        var contentType = exportFormat == ExportFormat.Csv ? "text/csv" : "application/json";
        var fileName = exportFormat == ExportFormat.Csv ? "audit-logs.csv" : "audit-logs.json";

        response.Headers.ContentDisposition = $"attachment; filename={fileName}";
        return Results.Stream(stream, contentType);
    }

    private static async Task<IResult> VerifyIntegrity(
        Guid id,
        [FromServices] IAuditService auditService)
    {
        var isValid = await auditService.VerifyIntegrityAsync(id).ConfigureAwait(false);

        return Results.Ok(new
        {
            auditLogId = id,
            isValid,
            message = isValid
                ? "Audit log integrity verified - no tampering detected."
                : "Audit log integrity check failed - possible tampering detected."
        });
    }
}
