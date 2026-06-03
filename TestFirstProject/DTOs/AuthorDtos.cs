namespace TestFirstProject.DTOs
{
    // --- Authors ---

    public record CreateAuthorRequest(
        string Name,
        string? Biography,
        string? Country
    );

    public record UpdateAuthorRequest(
        string Name,
        string? Biography,
        string? Country
    );

    public record AuthorDto(
        Guid Id,
        string Name,
        string? Biography,
        string? Country,
        DateTime CreatedAt,
        int BookCount
    );
}
