using TestFirstProject.DTOs.Catalog;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products");

        // GET /api/products — paginated list with filters
        group.MapGet("/", async (
            IProductService productService,
            Guid? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            bool? inStock,
            string? search,
            int page = 1,
            int pageSize = 20) =>
        {
            var filter = new ProductFilterRequest(categoryId, minPrice, maxPrice, inStock, search, page, pageSize);
            var result = await productService.GetProductsAsync(filter);
            return Results.Ok(result);
        });

        // GET /api/products/categories — list all categories
        group.MapGet("/categories", async (IProductService productService) =>
        {
            var categories = await productService.GetCategoriesAsync();
            return Results.Ok(categories);
        });

        // GET /api/products/{id} — product detail with variants and reviews
        group.MapGet("/{id:guid}", async (Guid id, IProductService productService) =>
        {
            var product = await productService.GetProductByIdAsync(id);
            return product == null ? Results.NotFound() : Results.Ok(product);
        });

        // POST /api/products — create product (Admin/Manager)
        group.MapPost("/", async (CreateProductRequest request, IProductService productService, HttpContext context) =>
        {
            var userId = context.GetUserId();
            var product = await productService.CreateProductAsync(request, userId);
            return Results.Created($"/api/products/{product.Id}", product);
        }).RequireAuthorization();

        // PUT /api/products/{id} — update product
        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, IProductService productService, HttpContext context) =>
        {
            var userId = context.GetUserId();
            var product = await productService.UpdateProductAsync(id, request, userId);
            return Results.Ok(product);
        }).RequireAuthorization();

        // POST /api/products/{id}/reviews — submit review
        group.MapPost("/{id:guid}/reviews", async (Guid id, CreateProductReviewRequest request, IProductService productService, HttpContext context) =>
        {
            var userId = context.GetUserId();
            var review = await productService.AddReviewAsync(id, request, userId);
            return Results.Created($"/api/products/{id}/reviews/{review.Id}", review);
        }).RequireAuthorization();
    }
}
