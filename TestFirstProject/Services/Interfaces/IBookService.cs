using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Business-logic contract for the Book Library feature.
    /// </summary>
    public interface IBookService
    {
        /// <summary>Adds a new book and returns its DTO representation.</summary>
        Task<BookDto> CreateAsync(CreateBookRequest request);

        /// <summary>
        /// Returns a paginated list of active (non-deleted) books.
        /// Optionally filters by <paramref name="genre"/> (case-insensitive).
        /// </summary>
        Task<PagedResult<BookDto>> GetPagedAsync(int page, int pageSize, string? genre);

        /// <summary>Returns a single active book by its ID, or throws <c>NotFoundException</c>.</summary>
        Task<BookDto> GetByIdAsync(Guid id);

        /// <summary>Soft-deletes a book by ID. Throws <c>NotFoundException</c> if not found.</summary>
        Task SoftDeleteAsync(Guid id);

        /// <summary>Updates the mutable fields of a book. Only non-null values in the request are applied.</summary>
        Task<BookDto> UpdateAsync(Guid id, UpdateBookRequest request);
    }
}
