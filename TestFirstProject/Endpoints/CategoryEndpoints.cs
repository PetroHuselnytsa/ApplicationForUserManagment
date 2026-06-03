using TestFirstProject.DTOs;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    /// <summary>
    /// Maps category management endpoints.
    /// </summary>
    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/categories")
                .WithTags("Categories");

            group.MapGet("/", async (ICategoryService categoryService) =>
            {
                var result = await categoryService.GetAllCategoriesAsync();
                return Results.Ok(result);
            })
            .WithName("GetCategories")
            .AllowAnonymous();

            group.MapGet("/{id:guid}", async (Guid id, ICategoryService categoryService) =>
            {
                var result = await categoryService.GetCategoryByIdAsync(id);
                return Results.Ok(result);
            })
            .WithName("GetCategoryById")
            .AllowAnonymous();

            group.MapPost("/", async (CreateCategoryRequest request, ICategoryService categoryService) =>
            {
                var result = await categoryService.CreateCategoryAsync(request);
                return Results.Created($"/api/categories/{result.Id}", result);
            })
            .WithName("CreateCategory")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

            group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, ICategoryService categoryService) =>
            {
                var result = await categoryService.UpdateCategoryAsync(id, request);
                return Results.Ok(result);
            })
            .WithName("UpdateCategory")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

            group.MapDelete("/{id:guid}", async (Guid id, ICategoryService categoryService) =>
            {
                await categoryService.DeleteCategoryAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteCategory")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
        }
    }
}
