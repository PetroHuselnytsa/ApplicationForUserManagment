using TestFirstProject.Models.Enums;

namespace TestFirstProject.DTOs.Catalog;

// --- Request DTOs ---

public record CreateProductRequest(
    string Name,
    string Description,
    string SKU,
    decimal BasePrice,
    decimal Weight,
    Guid CategoryId,
    string? ImageUrls,
    List<CreateProductVariantRequest>? Variants
);

public record UpdateProductRequest(
    string? Name,
    string? Description,
    decimal? BasePrice,
    decimal? Weight,
    Guid? CategoryId,
    string? ImageUrls,
    bool? IsActive
);

public record CreateProductVariantRequest(
    string SKU,
    string? Size,
    string? Color,
    string? Material,
    decimal PriceDelta
);

public record CreateProductReviewRequest(
    int Rating,
    string? ReviewText
);

public record ProductFilterRequest(
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? InStock = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20
);

// --- Response DTOs ---

public record ProductListDto(
    Guid Id,
    string Name,
    string Description,
    string SKU,
    decimal BasePrice,
    decimal? SalePrice,
    Guid CategoryId,
    string CategoryName,
    string? ImageUrls,
    double AverageRating,
    int ReviewCount,
    bool InStock
);

public record ProductDetailDto(
    Guid Id,
    string Name,
    string Description,
    string SKU,
    decimal BasePrice,
    decimal? SalePrice,
    decimal Weight,
    Guid CategoryId,
    string CategoryName,
    string? ImageUrls,
    bool IsActive,
    double AverageRating,
    int ReviewCount,
    DateTime CreatedAt,
    List<ProductVariantDto> Variants,
    List<ProductReviewDto> Reviews
);

public record ProductVariantDto(
    Guid Id,
    string SKU,
    string? Size,
    string? Color,
    string? Material,
    decimal PriceDelta,
    decimal FinalPrice,
    bool IsActive,
    int TotalStock
);

public record ProductReviewDto(
    Guid Id,
    Guid UserId,
    string Username,
    int Rating,
    string? ReviewText,
    bool IsVerifiedPurchase,
    DateTime CreatedAt
);

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    List<CategoryDto>? SubCategories
);
