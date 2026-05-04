namespace TestFirstProject.DTOs
{
    // --- Books ---

    /// <summary>Response DTO returned for every book read operation.</summary>
    public record BookDto(
        Guid Id,
        string Title,
        string Author,
        int Year,
        string Genre,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    /// <summary>Request body for creating a new book.</summary>
    public record CreateBookRequest(
        string Title,
        string Author,
        int Year,
        string Genre
    );

    /// <summary>Request body for updating an existing book's details.</summary>
    public record UpdateBookRequest(
        string? Title,
        string? Author,
        int? Year,
        string? Genre
    );

    /// <summary>Generic offset-based paginated result wrapper.</summary>
    public record PagedResult<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalCount
    );
}
