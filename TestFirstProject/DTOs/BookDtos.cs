namespace TestFirstProject.DTOs
{
    // --- Book Library ---

    public record CreateBookRequest(
        string Title,
        string Author,
        int Year,
        string Genre
    );

    public record UpdateBookRequest(
        string Title,
        string Author,
        int Year,
        string Genre
    );

    public record BookDto(
        Guid Id,
        string Title,
        string Author,
        int Year,
        string Genre,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
