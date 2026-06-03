namespace TestFirstProject.DTOs
{
    // --- Books ---

    public record CreateBookRequest(
        string Title,
        string Isbn,
        string? Publisher,
        int? PublicationYear,
        string? Genre,
        string? Description,
        string? CoverImageUrl,
        int? PageCount,
        string? Language,
        string? Edition,
        int TotalCopies,
        List<Guid>? AuthorIds,
        List<Guid>? CategoryIds
    );

    public record UpdateBookRequest(
        string Title,
        string Isbn,
        string? Publisher,
        int? PublicationYear,
        string? Genre,
        string? Description,
        string? CoverImageUrl,
        int? PageCount,
        string? Language,
        string? Edition,
        int TotalCopies,
        List<Guid>? AuthorIds,
        List<Guid>? CategoryIds
    );

    public record BookDto(
        Guid Id,
        string Title,
        string Isbn,
        string? Publisher,
        int? PublicationYear,
        string? Genre,
        string? Description,
        string? CoverImageUrl,
        int? PageCount,
        string? Language,
        string? Edition,
        int TotalCopies,
        int AvailableCopies,
        DateTime CreatedAt,
        List<AuthorSummaryDto> Authors,
        List<CategorySummaryDto> Categories
    );

    public record BookSummaryDto(
        Guid Id,
        string Title,
        string Isbn,
        int AvailableCopies
    );

    public record AuthorSummaryDto(Guid Id, string Name);

    public record CategorySummaryDto(Guid Id, string Name);
}
