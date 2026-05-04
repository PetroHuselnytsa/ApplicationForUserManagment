using Microsoft.EntityFrameworkCore;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Repositories;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class BookService : IBookService
    {
        private readonly IGenericRepository<Book> _repository;

        public BookService(IGenericRepository<Book> repository)
        {
            _repository = repository;
        }

        public async Task<BookDto> AddBookAsync(CreateBookRequest request)
        {
            ValidateBookRequest(request.Title, request.Author, request.Year, request.Genre);

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

        public async Task<PagedResult<BookDto>> GetBooksAsync(string? genre, int page = 1, int pageSize = 20)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _repository.Query();

            // Apply optional genre filter
            if (!string.IsNullOrWhiteSpace(genre))
            {
                query = query.Where(b => b.Genre.ToLower() == genre.Trim().ToLower());
            }

            query = query.OrderByDescending(b => b.CreatedAt);

            int totalCount = await query.CountAsync();

            var books = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookDto(
                    b.Id,
                    b.Title,
                    b.Author,
                    b.Year,
                    b.Genre,
                    b.CreatedAt,
                    b.UpdatedAt
                ))
                .ToListAsync();

            return new PagedResult<BookDto>(books, totalCount, page, pageSize);
        }

        public async Task<BookDto> GetBookByIdAsync(Guid id)
        {
            var book = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException("Book not found.");

            return MapToDto(book);
        }

        public async Task<BookDto> UpdateBookAsync(Guid id, UpdateBookRequest request)
        {
            ValidateBookRequest(request.Title, request.Author, request.Year, request.Genre);

            var book = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException("Book not found.");

            book.Title = request.Title.Trim();
            book.Author = request.Author.Trim();
            book.Year = request.Year;
            book.Genre = request.Genre.Trim();
            book.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(book);

            return MapToDto(book);
        }

        public async Task DeleteBookAsync(Guid id)
        {
            var book = await _repository.GetByIdAsync(id)
                ?? throw new NotFoundException("Book not found.");

            book.IsDeleted = true;
            book.DeletedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(book);
        }

        // --- Private helpers ---

        private static void ValidateBookRequest(string title, string author, int year, string genre)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ValidationException("Title is required.");

            if (title.Trim().Length > 500)
                throw new ValidationException("Title must not exceed 500 characters.");

            if (string.IsNullOrWhiteSpace(author))
                throw new ValidationException("Author is required.");

            if (author.Trim().Length > 300)
                throw new ValidationException("Author must not exceed 300 characters.");

            if (year < 1 || year > DateTime.UtcNow.Year + 1)
                throw new ValidationException($"Year must be between 1 and {DateTime.UtcNow.Year + 1}.");

            if (string.IsNullOrWhiteSpace(genre))
                throw new ValidationException("Genre is required.");

            if (genre.Trim().Length > 100)
                throw new ValidationException("Genre must not exceed 100 characters.");
        }

        private static BookDto MapToDto(Book book)
        {
            return new BookDto(
                book.Id,
                book.Title,
                book.Author,
                book.Year,
                book.Genre,
                book.CreatedAt,
                book.UpdatedAt
            );
        }
    }
}
