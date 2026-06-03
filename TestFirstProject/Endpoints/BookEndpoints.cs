using TestFirstProject.DTOs;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    /// <summary>
    /// Maps book catalog endpoints. Read operations are public,
    /// write operations require Admin role.
    /// </summary>
    public static class BookEndpoints
    {
        public static void MapBookEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/books")
                .WithTags("Books");

            // GET /books - list all books with optional filters
            group.MapGet("/", async (
                IBookService bookService,
                string? search,
                string? genre,
                Guid? categoryId) =>
            {
                var result = await bookService.GetAllBooksAsync(search, genre, categoryId);
                return Results.Ok(result);
            })
            .WithName("GetBooks")
            .AllowAnonymous();

            // GET /books/{id} - get book by ID
            group.MapGet("/{id:guid}", async (Guid id, IBookService bookService) =>
            {
                var result = await bookService.GetBookByIdAsync(id);
                return Results.Ok(result);
            })
            .WithName("GetBookById")
            .AllowAnonymous();

            // POST /books - create book (Admin only)
            group.MapPost("/", async (CreateBookRequest request, IBookService bookService) =>
            {
                var result = await bookService.CreateBookAsync(request);
                return Results.Created($"/api/books/{result.Id}", result);
            })
            .WithName("CreateBook")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

            // PUT /books/{id} - update book (Admin only)
            group.MapPut("/{id:guid}", async (Guid id, UpdateBookRequest request, IBookService bookService) =>
            {
                var result = await bookService.UpdateBookAsync(id, request);
                return Results.Ok(result);
            })
            .WithName("UpdateBook")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

            // DELETE /books/{id} - delete book (Admin only)
            group.MapDelete("/{id:guid}", async (Guid id, IBookService bookService) =>
            {
                await bookService.DeleteBookAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteBook")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
        }
    }
}
