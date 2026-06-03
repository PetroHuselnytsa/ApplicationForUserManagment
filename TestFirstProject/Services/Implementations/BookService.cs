using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class BookService : IBookService
    {
        private readonly PersonsContext _context;

        public BookService(PersonsContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<BookDto>> GetAllBooksAsync(
            string? searchTerm = null, string? genre = null, Guid? categoryId = null)
        {
            var query = _context.Books
                .AsNoTracking()
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .Include(b => b.BookCategories).ThenInclude(bc => bc.Category);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(term)
                    || b.Isbn.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(genre))
                query = query.Where(b => b.Genre == genre);

            if (categoryId.HasValue)
                query = query.Where(b => b.BookCategories.Any(bc => bc.CategoryId == categoryId));

            var books = await query.OrderBy(b => b.Title).ToListAsync();
            return books.Select(MapToBookDto).ToList();
        }

        public async Task<BookDto> GetBookByIdAsync(Guid id)
        {
            var book = await _context.Books
                .AsNoTracking()
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .FirstOrDefaultAsync(b => b.Id == id)
                ?? throw new NotFoundException("Book not found.");

            return MapToBookDto(book);
        }

        public async Task<BookDto> CreateBookAsync(CreateBookRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ValidationException("Title is required.");
            if (string.IsNullOrWhiteSpace(request.Isbn))
                throw new ValidationException("ISBN is required.");

            var existingIsbn = await _context.Books.AnyAsync(b => b.Isbn == request.Isbn);
            if (existingIsbn)
                throw new ConflictException("A book with this ISBN already exists.");

            var initialCopies = request.TotalCopies > 0 ? request.TotalCopies : 1;
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Isbn = request.Isbn,
                Publisher = request.Publisher,
                PublicationYear = request.PublicationYear,
                Genre = request.Genre,
                Description = request.Description,
                CoverImageUrl = request.CoverImageUrl,
                PageCount = request.PageCount,
                Language = request.Language,
                Edition = request.Edition,
                TotalCopies = initialCopies,
                AvailableCopies = initialCopies,
                CreatedAt = DateTime.UtcNow
            };

            await AddAuthorsToBookAsync(book, request.AuthorIds);
            await AddCategoriesToBookAsync(book, request.CategoryIds);

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return await GetBookByIdAsync(book.Id);
        }

        public async Task<BookDto> UpdateBookAsync(Guid id, UpdateBookRequest request)
        {
            var book = await _context.Books
                .Include(b => b.BookAuthors)
                .Include(b => b.BookCategories)
                .FirstOrDefaultAsync(b => b.Id == id)
                ?? throw new NotFoundException("Book not found.");

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ValidationException("Title is required.");
            if (string.IsNullOrWhiteSpace(request.Isbn))
                throw new ValidationException("ISBN is required.");

            if (request.Isbn != book.Isbn)
            {
                var existingIsbn = await _context.Books.AnyAsync(b => b.Isbn == request.Isbn && b.Id != id);
                if (existingIsbn)
                    throw new ConflictException("A book with this ISBN already exists.");
            }

            book.Title = request.Title;
            book.Isbn = request.Isbn;
            book.Publisher = request.Publisher;
            book.PublicationYear = request.PublicationYear;
            book.Genre = request.Genre;
            book.Description = request.Description;
            book.CoverImageUrl = request.CoverImageUrl;
            book.PageCount = request.PageCount;
            book.Language = request.Language;
            book.Edition = request.Edition;

            var copiesDiff = request.TotalCopies - book.TotalCopies;
            book.TotalCopies = request.TotalCopies > 0 ? request.TotalCopies : 1;
            book.AvailableCopies = Math.Max(0, book.AvailableCopies + copiesDiff);
            book.UpdatedAt = DateTime.UtcNow;

            book.BookAuthors.Clear();
            await AddAuthorsToBookAsync(book, request.AuthorIds);

            book.BookCategories.Clear();
            await AddCategoriesToBookAsync(book, request.CategoryIds);

            await _context.SaveChangesAsync();
            return await GetBookByIdAsync(book.Id);
        }

        public async Task DeleteBookAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id)
                ?? throw new NotFoundException("Book not found.");

            var hasActiveLoans = await _context.Loans.AnyAsync(l => l.BookId == id && l.Status == LoanStatus.Active);
            if (hasActiveLoans)
                throw new ConflictException("Cannot delete a book with active loans.");

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        private async Task AddAuthorsToBookAsync(Book book, IEnumerable<Guid>? authorIds)
        {
            if (authorIds?.Any() != true) return;

            foreach (var authorId in authorIds.Distinct())
            {
                if (!await _context.Authors.AnyAsync(a => a.Id == authorId))
                    throw new NotFoundException($"Author with ID {authorId} not found.");
                book.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
            }
        }

        private async Task AddCategoriesToBookAsync(Book book, IEnumerable<Guid>? categoryIds)
        {
            if (categoryIds?.Any() != true) return;

            foreach (var categoryId in categoryIds.Distinct())
            {
                if (!await _context.Categories.AnyAsync(c => c.Id == categoryId))
                    throw new NotFoundException($"Category with ID {categoryId} not found.");
                book.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
            }
        }

        private static BookDto MapToBookDto(Book book)
        {
            return new BookDto(
                book.Id,
                book.Title,
                book.Isbn,
                book.Publisher,
                book.PublicationYear,
                book.Genre,
                book.Description,
                book.CoverImageUrl,
                book.PageCount,
                book.Language,
                book.Edition,
                book.TotalCopies,
                book.AvailableCopies,
                book.CreatedAt,
                book.BookAuthors.Select(ba => new AuthorSummaryDto(ba.Author.Id, ba.Author.Name)).ToList(),
                book.BookCategories.Select(bc => new CategorySummaryDto(bc.Category.Id, bc.Category.Name)).ToList()
            );
        }
    }
}
