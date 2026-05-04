namespace TestFirstProject.DTOs
{
    // ────────────────────────────────────────────────────────────────────────────
    // Book Library DTOs
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>Request body for creating a new book.</summary>
    public record CreateBookRequest(
        string Title,
        string Author,
        int Year,
        string Genre
    );

    /// <summary>Request body for updating an existing book's details.</summary>
    public record UpdateBookRequest(
        string Title,
        string Author,
        int Year,
        string Genre
    );

    /// <summary>Response DTO for a single book. Excludes the soft-delete flag.</summary>
    public record BookResponse(
        Guid Id,
        string Title,
        string Author,
        int Year,
        string Genre,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
