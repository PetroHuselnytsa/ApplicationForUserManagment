using Microsoft.AspNetCore.Mvc;
using TestFirstProject.Services.Logging;

namespace TestFirstProject.Endpoints;

/// <summary>
/// Log search and analytics API endpoints.
/// </summary>
public static class LogEndpoints
{
    /// <summary>
    /// Maps log endpoints to the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapLogEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/logs");

        group.MapGet("/search", SearchLogs)
            .WithName("SearchLogs")
            .WithOpenApi();

        group.MapGet("/analytics/error-rate", GetErrorRate)
            .WithName("GetErrorRate")
            .WithOpenApi();

        group.MapGet("/analytics/request-volume", GetRequestVolume)
            .WithName("GetRequestVolume")
            .WithOpenApi();

        group.MapGet("/analytics/response-times", GetResponseTimes)
            .WithName("GetResponseTimes")
            .WithOpenApi();
    }

    private static async Task<IResult> SearchLogs(
        [FromQuery] string? level,
        [FromQuery] DateTime? fromTimestamp,
        [FromQuery] DateTime? toTimestamp,
        [FromQuery] string? correlationId,
        [FromQuery] string? userId,
        [FromQuery] string? searchText,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        [FromServices] ILogSearchService logSearchService)
    {
        var criteria = new LogSearchCriteria
        {
            Level = level,
            FromTimestamp = fromTimestamp,
            ToTimestamp = toTimestamp,
            CorrelationId = correlationId,
            UserId = userId,
            SearchText = searchText,
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 1000)
        };

        var result = await logSearchService.SearchLogsAsync(criteria).ConfigureAwait(false);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetErrorRate(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string interval = "hour",
        [FromServices] ILogAnalyticsService analyticsService)
    {
        var validIntervals = new[] { "hour", "day", "week" };
        if (!validIntervals.Contains(interval.ToLowerInvariant()))
        {
            return Results.BadRequest(new { error = "Invalid interval. Use 'hour', 'day', or 'week'." });
        }

        var result = await analyticsService.GetErrorRateAsync(fromDate, toDate, interval).ConfigureAwait(false);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetRequestVolume(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? endpoint,
        [FromQuery] string interval = "hour",
        [FromServices] ILogAnalyticsService analyticsService)
    {
        var validIntervals = new[] { "hour", "day", "week" };
        if (!validIntervals.Contains(interval.ToLowerInvariant()))
        {
            return Results.BadRequest(new { error = "Invalid interval. Use 'hour', 'day', or 'week'." });
        }

        var result = await analyticsService.GetRequestVolumeAsync(fromDate, toDate, endpoint, interval).ConfigureAwait(false);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetResponseTimes(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? endpoint,
        [FromServices] ILogAnalyticsService analyticsService)
    {
        var result = await analyticsService.GetResponseTimesAsync(fromDate, toDate, endpoint).ConfigureAwait(false);
        return Results.Ok(result);
    }
}
