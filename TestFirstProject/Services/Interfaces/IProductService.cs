using TestFirstProject.DTOs.Catalog;
using TestFirstProject.DTOs.Common;

namespace TestFirstProject.Services.Interfaces;

public interface IProductService
{
    Task<PaginatedResult<ProductListDto>> GetProductsAsync(ProductFilterRequest filter);
    Task<ProductDetailDto?> GetProductByIdAsync(Guid productId);
    Task<ProductDetailDto> CreateProductAsync(CreateProductRequest request, Guid userId);
    Task<ProductDetailDto> UpdateProductAsync(Guid productId, UpdateProductRequest request, Guid userId);
    Task<ProductReviewDto> AddReviewAsync(Guid productId, CreateProductReviewRequest request, Guid userId);
    Task<List<CategoryDto>> GetCategoriesAsync();
}
