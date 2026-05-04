using Microsoft.EntityFrameworkCore;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Repositories;

namespace TestFirstProject.Endpoints
{
    /// <summary>
    /// Maps all Book Library API endpoints under <c>/api/books</c>.
    /// No authentication is required — these endpoints are publicly accessible.
    /// </summary>
    public static class BookEndpoints
    {
        public static void MapBookEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/books")
                           .WithTags("Books");

            // POST /api/books — Add a new book
            group.MapPost("/", async (
                BookRequest request,
                IRepository<Book> repo) =>
            {
                ValidateBookRequest(request);

                var now = DateTime.UtcNow;
                var book = new Book
                {
                    Id        = Guid.NewGuid(),
                    Title     = request.Title.Trim(),
                    Author    = request.Author.Trim(),
                    Year      = request.Year,
                    Genre     = request.Genre.Trim(),
                    IsDeleted = false,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await repo.AddAsync(book);

                return Results.Created($"/api/books/{book.Id}", MapToResponse(book));
            })
            .WithName("CreateBook");

            // GET /api/books — Paginated list with optional genre filter
            group.MapGet("/", async (
                IRepository<Book> repo,
                string? genre = null,
                int page = 1,
                int pageSize = 20) =>
            {
                // Clamp page size between 1 and 100
                page     = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 100);

                var query = repo.GetAll()
                                .Where(b => !b.IsDeleted);

                if (!string.IsNullOrWhiteSpace(genre))
                {
                    // Case-insensitive match via EF.Functions.ILike (PostgreSQL extension)
                    query = query.Where(b => EF.Functions.ILike(b.Genre, genre.Trim()));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(b => b.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(b => MapToResponse(b))
                    .ToListAsync();

                var result = new PagedResult<BookResponse>(items, totalCount, page, pageSize);
                return Results.Ok(result);
            })
            .WithName("GetBooks");

            // GET /api/books/{id:guid} — Single book by ID
            group.MapGet("/{id:guid}", async (
                Guid id,
                IRepository<Book> repo) =>
            {
                var book = await repo.GetByIdAsync(id);

                if (book is null || book.IsDeleted)
                    throw new NotFoundException($"Book with id '{id}' was not found.");

                return Results.Ok(MapToResponse(book));
            })
            .WithName("GetBookById");

            // PUT /api/books/{id:guid} — Update book details
            group.MapPut("/{id:guid}", async (
                Guid id,
                BookRequest request,
                IRepository<Book> repo) =>
            {
                ValidateBookRequest(request);

                var book = await repo.GetByIdAsync(id);

                if (book is null || book.IsDeleted)
                    throw new NotFoundException($"Book with id '{id}' was not found.");

                book.Title     = request.Title.Trim();
                book.Author    = request.Author.Trim();
                book.Year      = request.Year;
                book.Genre     = request.Genre.Trim();
                book.UpdatedAt = DateTime.UtcNow;

                await repo.UpdateAsync(book);

                return Results.Ok(MapToResponse(book));
            })
            .WithName("UpdateBook");

            // DELETE /api/books/{id:guid} — Soft-delete a book
            group.MapDelete("/{id:guid}", async (
                Guid id,
                IRepository<Book> repo) =>
            {
                var book = await repo.GetByIdAsync(id);

                if (book is null || book.IsDeleted)
                    throw new NotFoundException($"Book with id '{id}' was not found.");

                book.IsDeleted = true;
                book.UpdatedAt = DateTime.UtcNow;

                await repo.UpdateAsync(book);

                return Results.NoContent();
            })
            .WithName("DeleteBook");
        }

        // ─── Private helpers ───────────────────────────────────────────────────────

        /// <summary>Validates shared fields across create and update requests.</summary>
        private static void ValidateBookRequest(BookRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ValidationException("Title is required.");

            if (string.IsNullOrWhiteSpace(request.Author))
                throw new ValidationException("Author is required.");

            if (string.IsNullOrWhiteSpace(request.Genre))
                throw new ValidationException("Genre is required.");

            if (request.Year < 1 || request.Year > DateTime.UtcNow.Year + 5)
                throw new ValidationException($"Year must be between 1 and {DateTime.UtcNow.Year + 5}.");
        }

        /// <summary>Maps a <see cref="Book"/> entity to its API response record.</summary>
        private static BookResponse MapToResponse(Book book) =>
            new(book.Id, book.Title, book.Author, book.Year, book.Genre, book.CreatedAt, book.UpdatedAt);
    }
}
