using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Handles book library CRUD operations including soft-delete
    /// and paginated listing with optional genre filter.
    /// </summary>
    public interface IBookService
    {
        /// <summary>Add a new book to the library.</summary>
        Task<BookDto> AddBookAsync(CreateBookRequest request);

        /// <summary>Get a paginated list of books with optional genre filter.</summary>
        Task<PagedResult<BookDto>> GetBooksAsync(string? genre, int page = 1, int pageSize = 20);

        /// <summary>Get a single book by its ID.</summary>
        Task<BookDto> GetBookByIdAsync(Guid id);

        /// <summary>Update an existing book's details.</summary>
        Task<BookDto> UpdateBookAsync(Guid id, UpdateBookRequest request);

        /// <summary>Soft-delete a book (sets IsDeleted and DeletedAt).</summary>
        Task DeleteBookAsync(Guid id);
    }
}
