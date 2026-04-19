namespace TestFirstProject.DTOs
{
    // --- Product Catalog ---

    public record CreateProductRequest(
        string Name,
        string? Description,
        string Sku,
        decimal BasePrice,
        decimal Weight,
        Guid CategoryId,
        List<string>? ImageUrls,
        List<CreateVariantRequest>? Variants);

    public record CreateVariantRequest(
        string Sku,
        string? Size,
        string? Color,
        string? Material,
        decimal PriceDelta);

    public record UpdateProductRequest(
        string? Name,
        string? Description,
        decimal? BasePrice,
        decimal? Weight,
        Guid? CategoryId,
        List<string>? ImageUrls,
        bool? IsActive);

    public record ProductResponse(
        Guid Id,
        string Name,
        string? Description,
        string Sku,
        decimal BasePrice,
        decimal Weight,
        bool IsActive,
        Guid CategoryId,
        string CategoryName,
        List<string> ImageUrls,
        double AverageRating,
        int ReviewCount,
        List<VariantResponse> Variants);

    public record ProductListResponse(
        Guid Id,
        string Name,
        string Sku,
        decimal BasePrice,
        bool IsActive,
        string CategoryName,
        List<string> ImageUrls,
        double AverageRating,
        bool InStock);

    public record VariantResponse(
        Guid Id,
        string Sku,
        string? Size,
        string? Color,
        string? Material,
        decimal PriceDelta,
        decimal FinalPrice,
        bool IsActive,
        int AvailableStock);

    public record CreateReviewRequest(
        int Rating,
        string? Text);

    public record ReviewResponse(
        Guid Id,
        string Username,
        int Rating,
        string? Text,
        bool IsVerifiedPurchase,
        DateTime CreatedAt);

    public record PaginatedResponse<T>(
        List<T> Items,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages);
}
