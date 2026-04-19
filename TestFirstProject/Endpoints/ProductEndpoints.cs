using TestFirstProject.DTOs;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    public static class ProductEndpoints
    {
        public static void MapProductEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/products")
                           .WithTags("Products");

            group.MapGet("/", async (
                IProductService productService,
                int? page,
                int? pageSize,
                Guid? categoryId,
                decimal? minPrice,
                decimal? maxPrice,
                bool? inStock,
                string? search) =>
            {
                var result = await productService.GetProductsAsync(
                    page ?? 1, pageSize ?? 20, categoryId, minPrice, maxPrice, inStock, search);
                return Results.Ok(result);
            })
            .WithName("GetProducts")
            .AllowAnonymous();

            group.MapGet("/{id:guid}", async (Guid id, IProductService productService) =>
            {
                var result = await productService.GetProductByIdAsync(id);
                return Results.Ok(result);
            })
            .WithName("GetProductById")
            .AllowAnonymous();

            group.MapPost("/", async (CreateProductRequest request, IProductService productService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await productService.CreateProductAsync(request, userId);
                return Results.Created($"/api/products/{result.Id}", result);
            })
            .WithName("CreateProduct")
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"));

            group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, IProductService productService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await productService.UpdateProductAsync(id, request, userId);
                return Results.Ok(result);
            })
            .WithName("UpdateProduct")
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"));

            group.MapPost("/{id:guid}/reviews", async (Guid id, CreateReviewRequest request, IProductService productService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await productService.AddReviewAsync(id, request, userId);
                return Results.Created($"/api/products/{id}/reviews/{result.Id}", result);
            })
            .WithName("AddProductReview")
            .RequireAuthorization();
        }
    }
}
