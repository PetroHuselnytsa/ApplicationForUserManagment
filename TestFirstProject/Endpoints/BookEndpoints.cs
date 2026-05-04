using TestFirstProject.DTOs;
using TestFirstProject.Models;
using TestFirstProject.Repositories.Interfaces;

namespace TestFirstProject.Endpoints
{
    /// <summary>
    /// Maps the Book Library REST endpoints under <c>/api/books</c>.
    /// No authentication is required. All database operations are async.
    /// Soft-deleted books are hidden from all read operations.
    /// </summary>
    public static class BookEndpoints
    {
        public static void MapBookEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/books")
                           .WithTags("Books");

            // ── POST /api/books ─────────────────────────────────────────────────
            // Add a new book.
            group.MapPost("/", async (
                CreateBookRequest request,
                IRepository<Book> repo) =>
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Results.BadRequest("Title is required.");
                if (string.IsNullOrWhiteSpace(request.Author))
                    return Results.BadRequest("Author is required.");
                if (string.IsNullOrWhiteSpace(request.Genre))
                    return Results.BadRequest("Genre is required.");

                var now = DateTime.UtcNow;
                var book = new Book
                {
                    Title = request.Title.Trim(),
                    Author = request.Author.Trim(),
                    Year = request.Year,
                    Genre = request.Genre.Trim(),
                    IsDeleted = false,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await repo.AddAsync(book);
                await repo.SaveChangesAsync();

                return Results.Created($"/api/books/{book.Id}", MapToResponse(book));
            })
            .WithName("CreateBook");

            // ── GET /api/books ──────────────────────────────────────────────────
            // Paginated list with optional genre filter.
            group.MapGet("/", async (
                IRepository<Book> repo,
                string? genre = null,
                int page = 1,
                int pageSize = 20) =>
            {
                // Clamp page size to a sensible range
                pageSize = Math.Clamp(pageSize, 1, 100);
                page = Math.Max(1, page);

                // Always exclude soft-deleted records
                System.Linq.Expressions.Expression<Func<Book, bool>> filter = string.IsNullOrWhiteSpace(genre)
                    ? b => !b.IsDeleted
                    : b => !b.IsDeleted && b.Genre == genre;

                var items = await repo.GetAllAsync(filter, page, pageSize);
                var total = await repo.CountAsync(filter);

                var result = new PagedResult<BookResponse>(
                    Items: items.Select(MapToResponse).ToList(),
                    TotalCount: total,
                    Page: page,
                    PageSize: pageSize
                );

                return Results.Ok(result);
            })
            .WithName("GetBooks");

            // ── GET /api/books/{id} ─────────────────────────────────────────────
            // Get a single book by ID.
            group.MapGet("/{id:guid}", async (
                Guid id,
                IRepository<Book> repo) =>
            {
                var book = await repo.GetByIdAsync(id);

                if (book is null || book.IsDeleted)
                    return Results.NotFound(new { message = $"Book '{id}' not found." });

                return Results.Ok(MapToResponse(book));
            })
            .WithName("GetBookById");

            // ── DELETE /api/books/{id} ──────────────────────────────────────────
            // Soft-delete a book (sets IsDeleted = true, row is kept).
            group.MapDelete("/{id:guid}", async (
                Guid id,
                IRepository<Book> repo) =>
            {
                var book = await repo.GetByIdAsync(id);

                if (book is null || book.IsDeleted)
                    return Results.NotFound(new { message = $"Book '{id}' not found." });

                repo.Delete(book); // ISoftDeletable → sets IsDeleted = true
                await repo.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteBook");

            // ── PUT /api/books/{id} ─────────────────────────────────────────────
            // Update a book's details.
            group.MapPut("/{id:guid}", async (
                Guid id,
                UpdateBookRequest request,
                IRepository<Book> repo) =>
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Results.BadRequest("Title is required.");
                if (string.IsNullOrWhiteSpace(request.Author))
                    return Results.BadRequest("Author is required.");
                if (string.IsNullOrWhiteSpace(request.Genre))
                    return Results.BadRequest("Genre is required.");

                var book = await repo.GetByIdAsync(id);

                if (book is null || book.IsDeleted)
                    return Results.NotFound(new { message = $"Book '{id}' not found." });

                book.Title = request.Title.Trim();
                book.Author = request.Author.Trim();
                book.Year = request.Year;
                book.Genre = request.Genre.Trim();
                book.UpdatedAt = DateTime.UtcNow;

                repo.Update(book);
                await repo.SaveChangesAsync();

                return Results.Ok(MapToResponse(book));
            })
            .WithName("UpdateBook");
        }

        // ── Helper ──────────────────────────────────────────────────────────────

        private static BookResponse MapToResponse(Book book) =>
            new(
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
