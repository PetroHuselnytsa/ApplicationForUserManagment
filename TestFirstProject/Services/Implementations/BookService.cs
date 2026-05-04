using Microsoft.EntityFrameworkCore;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Repositories;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    /// <summary>
    /// Implements <see cref="IBookService"/> using a generic <see cref="IRepository{Book}"/>.
    /// All database operations are async and soft-deleted books are transparently excluded.
    /// </summary>
    public class BookService : IBookService
    {
        private readonly IRepository<Book> _repository;
        private const int MinYear = 1000;

        public BookService(IRepository<Book> repository)
        {
            _repository = repository;
        }

        /// <inheritdoc/>
        public async Task<BookDto> CreateAsync(CreateBookRequest request)
        {
            ValidateRequest(request.Title, request.Author, request.Year, request.Genre);

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Author = request.Author.Trim(),
                Year = request.Year,
                Genre = request.Genre.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(book);

            return MapToDto(book);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<BookDto>> GetPagedAsync(int page, int pageSize, string? genre)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _repository.Query(b => !b.IsDeleted);

            // Optional genre filter — case-insensitive
            if (!string.IsNullOrWhiteSpace(genre))
            {
                var normalised = genre.Trim().ToLowerInvariant();
                query = query.Where(b => b.Genre.ToLower() == normalised);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.Title)
                .ThenBy(b => b.Author)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<BookDto>(
                items.Select(MapToDto).ToList(),
                page,
                pageSize,
                totalCount
            );
        }

        /// <inheritdoc/>
        public async Task<BookDto> GetByIdAsync(Guid id)
            => MapToDto(await GetActiveBookOrThrowAsync(id));

        /// <inheritdoc/>
        public async Task SoftDeleteAsync(Guid id)
        {
            var book = await GetActiveBookOrThrowAsync(id);

            book.IsDeleted = true;
            book.UpdatedAt = DateTime.UtcNow;

            _repository.Update(book);
            await _repository.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<BookDto> UpdateAsync(Guid id, UpdateBookRequest request)
        {
            var book = await GetActiveBookOrThrowAsync(id);

            // Apply only the non-null fields provided in the request
            if (request.Title != null)
                book.Title = ValidateAndTrim(request.Title, "Title");

            if (request.Author != null)
                book.Author = ValidateAndTrim(request.Author, "Author");

            if (request.Year.HasValue)
            {
                int maxYear = DateTime.UtcNow.Year;
                if (request.Year.Value < MinYear || request.Year.Value > maxYear)
                    throw new ValidationException($"Year must be between {MinYear} and {maxYear}.");
                book.Year = request.Year.Value;
            }

            if (request.Genre != null)
                book.Genre = ValidateAndTrim(request.Genre, "Genre");

            book.UpdatedAt = DateTime.UtcNow;

            _repository.Update(book);
            await _repository.SaveChangesAsync();

            return MapToDto(book);
        }

        // --- Private helpers ---

        private async Task<Book> GetActiveBookOrThrowAsync(Guid id)
        {
            var book = await _repository.GetByIdAsync(id);
            if (book is null || book.IsDeleted)
                throw new NotFoundException($"Book with ID '{id}' was not found.");
            return book;
        }

        /// <summary>Throws <see cref="ValidationException"/> if the value is blank; otherwise returns its trimmed form.</summary>
        private static string ValidateAndTrim(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException($"{fieldName} cannot be empty.");
            return value.Trim();
        }

        private static void ValidateRequest(string title, string author, int year, string genre)
        {
            ValidateAndTrim(title, "Title");
            ValidateAndTrim(author, "Author");
            ValidateAndTrim(genre, "Genre");

            int maxYear = DateTime.UtcNow.Year;
            if (year < MinYear || year > maxYear)
                throw new ValidationException($"Year must be between {MinYear} and {maxYear}.");
        }

        private static BookDto MapToDto(Book book) =>
            new(book.Id, book.Title, book.Author, book.Year, book.Genre, book.CreatedAt, book.UpdatedAt);
    }
}
