using TestFirstProject.DTOs;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    /// <summary>
    /// Maps the Book Library REST endpoints to the ASP.NET Core Minimal API pipeline.
    /// All routes are grouped under <c>/api/books</c> and require no authentication.
    /// </summary>
    public static class BookEndpoints
    {
        public static void MapBookEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/books")
                           .WithTags("Books");

            // POST /api/books — add a new book
            group.MapPost("/", async (CreateBookRequest request, IBookService bookService) =>
            {
                var book = await bookService.CreateAsync(request);
                return Results.Created($"/api/books/{book.Id}", book);
            })
            .WithName("CreateBook");

            // GET /api/books — paginated list with optional genre filter
            group.MapGet("/", async (
                IBookService bookService,
                int page = 1,
                int pageSize = 20,
                string? genre = null) =>
            {
                var result = await bookService.GetPagedAsync(page, pageSize, genre);
                return Results.Ok(result);
            })
            .WithName("GetBooks");

            // GET /api/books/{id} — single book by ID
            group.MapGet("/{id:guid}", async (Guid id, IBookService bookService) =>
            {
                var book = await bookService.GetByIdAsync(id);
                return Results.Ok(book);
            })
            .WithName("GetBookById");

            // PUT /api/books/{id} — update book details (partial update: only non-null fields applied)
            group.MapPut("/{id:guid}", async (Guid id, UpdateBookRequest request, IBookService bookService) =>
            {
                var book = await bookService.UpdateAsync(id, request);
                return Results.Ok(book);
            })
            .WithName("UpdateBook");

            // DELETE /api/books/{id} — soft-delete a book
            group.MapDelete("/{id:guid}", async (Guid id, IBookService bookService) =>
            {
                await bookService.SoftDeleteAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteBook");
        }
    }
}
