using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Handles book catalog management operations.
    /// </summary>
    public interface IBookService
    {
        Task<IReadOnlyList<BookDto>> GetAllBooksAsync(string? searchTerm = null, string? genre = null, Guid? categoryId = null);
        Task<BookDto> GetBookByIdAsync(Guid id);
        Task<BookDto> CreateBookAsync(CreateBookRequest request);
        Task<BookDto> UpdateBookAsync(Guid id, UpdateBookRequest request);
        Task DeleteBookAsync(Guid id);
    }
}
