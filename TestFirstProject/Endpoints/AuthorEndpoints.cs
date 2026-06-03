using TestFirstProject.DTOs;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    /// <summary>
    /// Maps author management endpoints.
    /// </summary>
    public static class AuthorEndpoints
    {
        public static void MapAuthorEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/authors")
                .WithTags("Authors");

            group.MapGet("/", async (IAuthorService authorService, string? search) =>
            {
                var result = await authorService.GetAllAuthorsAsync(search);
                return Results.Ok(result);
            })
            .WithName("GetAuthors")
            .AllowAnonymous();

            group.MapGet("/{id:guid}", async (Guid id, IAuthorService authorService) =>
            {
                var result = await authorService.GetAuthorByIdAsync(id);
                return Results.Ok(result);
            })
            .WithName("GetAuthorById")
            .AllowAnonymous();

            group.MapGet("/{id:guid}/books", async (Guid id, IAuthorService authorService) =>
            {
                var result = await authorService.GetBooksByAuthorAsync(id);
                return Results.Ok(result);
            })
            .WithName("GetBooksByAuthor")
            .AllowAnonymous();

            group.MapPost("/", async (CreateAuthorRequest request, IAuthorService authorService) =>
            {
                var result = await authorService.CreateAuthorAsync(request);
                return Results.Created($"/api/authors/{result.Id}", result);
            })
            .WithName("CreateAuthor")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

            group.MapPut("/{id:guid}", async (Guid id, UpdateAuthorRequest request, IAuthorService authorService) =>
            {
                var result = await authorService.UpdateAuthorAsync(id, request);
                return Results.Ok(result);
            })
            .WithName("UpdateAuthor")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

            group.MapDelete("/{id:guid}", async (Guid id, IAuthorService authorService) =>
            {
                await authorService.DeleteAuthorAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteAuthor")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
        }
    }
}
