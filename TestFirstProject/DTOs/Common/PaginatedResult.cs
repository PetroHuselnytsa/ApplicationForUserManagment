namespace TestFirstProject.DTOs.Common;

/// <summary>
/// Generic paginated result wrapper for API responses.
/// </summary>
public record PaginatedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
