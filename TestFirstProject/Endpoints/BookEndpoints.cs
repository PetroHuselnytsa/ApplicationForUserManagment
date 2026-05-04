using TestFirstProject.DTOs;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    /// <summary>
    /// Maps book library endpoints.
    /// Provides CRUD operations with soft-delete and paginated listing with optional genre filter.
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
                var result = await bookService.AddBookAsync(request);
                return Results.Created($"/api/books/{result.Id}", result);
            })
            .WithName("AddBook");

            // GET /api/books — paginated list with optional genre filter
            group.MapGet("/", async (
                IBookService bookService,
                string? genre,
                int page = 1,
                int pageSize = 20) =>
            {
                var result = await bookService.GetBooksAsync(genre, page, pageSize);
                return Results.Ok(result);
            })
            .WithName("GetBooks");

            // GET /api/books/{id} — get a single book by ID
            group.MapGet("/{id:guid}", async (Guid id, IBookService bookService) =>
            {
                var result = await bookService.GetBookByIdAsync(id);
                return Results.Ok(result);
            })
            .WithName("GetBookById");

            // PUT /api/books/{id} — update a book's details
            group.MapPut("/{id:guid}", async (Guid id, UpdateBookRequest request, IBookService bookService) =>
            {
                var result = await bookService.UpdateBookAsync(id, request);
                return Results.Ok(result);
            })
            .WithName("UpdateBook");

            // DELETE /api/books/{id} — soft-delete a book
            group.MapDelete("/{id:guid}", async (Guid id, IBookService bookService) =>
            {
                await bookService.DeleteBookAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteBook");
        }
    }
}
