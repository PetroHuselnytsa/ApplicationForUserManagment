using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class AuthorService : IAuthorService
    {
        private readonly PersonsContext _context;

        public AuthorService(PersonsContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<AuthorDto>> GetAllAuthorsAsync(string? searchTerm = null)
        {
            var query = _context.Authors
                .AsNoTracking()
                .Include(a => a.BookAuthors);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(a => a.Name.ToLower().Contains(term));
            }

            var authors = await query.OrderBy(a => a.Name).ToListAsync();
            return authors.Select(MapToAuthorDto).ToList();
        }

        public async Task<AuthorDto> GetAuthorByIdAsync(Guid id)
        {
            var author = await _context.Authors
                .AsNoTracking()
                .Include(a => a.BookAuthors)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new NotFoundException("Author not found.");

            return MapToAuthorDto(author);
        }

        public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Name is required.");

            var author = new Author
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Biography = request.Biography,
                Country = request.Country,
                CreatedAt = DateTime.UtcNow
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            return MapToAuthorDto(author);
        }

        public async Task<AuthorDto> UpdateAuthorAsync(Guid id, UpdateAuthorRequest request)
        {
            var author = await _context.Authors
                .Include(a => a.BookAuthors)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new NotFoundException("Author not found.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Name is required.");

            author.Name = request.Name;
            author.Biography = request.Biography;
            author.Country = request.Country;

            await _context.SaveChangesAsync();
            return MapToAuthorDto(author);
        }

        public async Task DeleteAuthorAsync(Guid id)
        {
            var author = await _context.Authors.FindAsync(id)
                ?? throw new NotFoundException("Author not found.");

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<BookSummaryDto>> GetBooksByAuthorAsync(Guid authorId)
        {
            var exists = await _context.Authors.AnyAsync(a => a.Id == authorId);
            if (!exists)
                throw new NotFoundException("Author not found.");

            var books = await _context.BookAuthors
                .AsNoTracking()
                .Where(ba => ba.AuthorId == authorId)
                .Select(ba => new BookSummaryDto(
                    ba.Book.Id,
                    ba.Book.Title,
                    ba.Book.Isbn,
                    ba.Book.AvailableCopies
                ))
                .ToListAsync();

            return books;
        }

        private static AuthorDto MapToAuthorDto(Author author)
        {
            return new AuthorDto(
                author.Id,
                author.Name,
                author.Biography,
                author.Country,
                author.CreatedAt,
                author.BookAuthors.Count
            );
        }
    }
}
