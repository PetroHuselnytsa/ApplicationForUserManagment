namespace TestFirstProject.DTOs
{
    // ─── Request DTOs ────────────────────────────────────────────────────────────

    /// <summary>Payload for creating or updating a book.</summary>
    public record BookRequest(
        string Title,
        string Author,
        int Year,
        string Genre
    );

    // ─── Response DTOs ───────────────────────────────────────────────────────────

    /// <summary>Representation of a book returned by the API.</summary>
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
