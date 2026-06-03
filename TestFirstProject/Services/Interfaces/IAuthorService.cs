using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Handles author management operations.
    /// </summary>
    public interface IAuthorService
    {
        Task<IReadOnlyList<AuthorDto>> GetAllAuthorsAsync(string? searchTerm = null);
        Task<AuthorDto> GetAuthorByIdAsync(Guid id);
        Task<AuthorDto> CreateAuthorAsync(CreateAuthorRequest request);
        Task<AuthorDto> UpdateAuthorAsync(Guid id, UpdateAuthorRequest request);
        Task DeleteAuthorAsync(Guid id);
        Task<IReadOnlyList<BookSummaryDto>> GetBooksByAuthorAsync(Guid authorId);
    }
}
