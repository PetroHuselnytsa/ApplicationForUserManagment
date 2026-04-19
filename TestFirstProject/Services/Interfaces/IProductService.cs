using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    public interface IProductService
    {
        Task<PaginatedResponse<ProductListResponse>> GetProductsAsync(
            int page, int pageSize, Guid? categoryId, decimal? minPrice, decimal? maxPrice, bool? inStock, string? search);
        Task<ProductResponse> GetProductByIdAsync(Guid id);
        Task<ProductResponse> CreateProductAsync(CreateProductRequest request, Guid userId);
        Task<ProductResponse> UpdateProductAsync(Guid id, UpdateProductRequest request, Guid userId);
        Task<ReviewResponse> AddReviewAsync(Guid productId, CreateReviewRequest request, Guid userId);
    }
}
