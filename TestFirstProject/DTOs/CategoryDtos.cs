namespace TestFirstProject.DTOs
{
    // --- Categories ---

    public record CreateCategoryRequest(
        string Name,
        string? Description
    );

    public record UpdateCategoryRequest(
        string Name,
        string? Description
    );

    public record CategoryDto(
        Guid Id,
        string Name,
        string? Description,
        DateTime CreatedAt,
        int BookCount
    );
}
