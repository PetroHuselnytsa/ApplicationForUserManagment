using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Catalog;
using TestFirstProject.DTOs.Common;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations;

public class ProductService : IProductService
{
    private readonly PersonsContext _context;
    private readonly IPromotionService _promotionService;

    public ProductService(PersonsContext context, IPromotionService promotionService)
    {
        _context = context;
        _promotionService = promotionService;
    }

    public async Task<PaginatedResult<ProductListDto>> GetProductsAsync(ProductFilterRequest filter)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
                .ThenInclude(v => v.StockEntries)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive);

        // Apply filters
        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.BasePrice >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.BasePrice <= filter.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Name.ToLower().Contains(filter.Search.ToLower())
                                  || p.Description.ToLower().Contains(filter.Search.ToLower()));

        if (filter.InStock == true)
            query = query.Where(p => p.Variants.Any(v => v.StockEntries.Any(s => s.QuantityOnHand - s.QuantityReserved > 0)));

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var items = new List<ProductListDto>();
        foreach (var p in products)
        {
            var salePrice = await _promotionService.GetFlashSalePriceAsync(p.Id);
            var inStock = p.Variants.Any(v => v.StockEntries.Any(s => s.QuantityOnHand - s.QuantityReserved > 0));
            var avgRating = p.Reviews.Count > 0 ? p.Reviews.Average(r => r.Rating) : 0;

            items.Add(new ProductListDto(
                p.Id, p.Name, p.Description, p.SKU, p.BasePrice, salePrice,
                p.CategoryId, p.Category.Name, p.ImageUrls,
                avgRating, p.Reviews.Count, inStock
            ));
        }

        return new PaginatedResult<ProductListDto>(
            items, totalCount, filter.Page, filter.PageSize,
            (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        );
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(Guid productId)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
                .ThenInclude(v => v.StockEntries)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return null;

        var salePrice = await _promotionService.GetFlashSalePriceAsync(product.Id);
        var avgRating = product.Reviews.Count > 0 ? product.Reviews.Average(r => r.Rating) : 0;

        var variants = product.Variants.Select(v => new ProductVariantDto(
            v.Id, v.SKU, v.Size, v.Color, v.Material, v.PriceDelta,
            product.BasePrice + v.PriceDelta, v.IsActive,
            v.StockEntries.Sum(s => s.QuantityOnHand - s.QuantityReserved)
        )).ToList();

        var reviews = product.Reviews.Select(r => new ProductReviewDto(
            r.Id, r.UserId, r.User.Username, r.Rating, r.ReviewText,
            r.IsVerifiedPurchase, r.CreatedAt
        )).OrderByDescending(r => r.CreatedAt).ToList();

        return new ProductDetailDto(
            product.Id, product.Name, product.Description, product.SKU,
            product.BasePrice, salePrice, product.Weight,
            product.CategoryId, product.Category.Name,
            product.ImageUrls, product.IsActive, avgRating, product.Reviews.Count,
            product.CreatedAt, variants, reviews
        );
    }

    public async Task<ProductDetailDto> CreateProductAsync(CreateProductRequest request, Guid userId)
    {
        // Validate category exists
        var category = await _context.Categories.FindAsync(request.CategoryId)
            ?? throw new NotFoundException($"Category with ID '{request.CategoryId}' not found.");

        // Check SKU uniqueness
        if (await _context.Products.AnyAsync(p => p.SKU == request.SKU))
            throw new ConflictException($"Product with SKU '{request.SKU}' already exists.");

        var now = DateTime.UtcNow;
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            SKU = request.SKU,
            BasePrice = request.BasePrice,
            Weight = request.Weight,
            CategoryId = request.CategoryId,
            ImageUrls = request.ImageUrls ?? string.Empty,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Products.Add(product);

        // Add variants if provided
        if (request.Variants != null)
        {
            // Check for duplicate SKUs within the request itself
            var duplicateSkus = request.Variants.GroupBy(v => v.SKU).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateSkus.Any())
                throw new ValidationException($"Duplicate variant SKUs in request: {string.Join(", ", duplicateSkus)}");

            foreach (var vr in request.Variants)
            {
                if (await _context.ProductVariants.AnyAsync(v => v.SKU == vr.SKU))
                    throw new ConflictException($"Product variant with SKU '{vr.SKU}' already exists.");

                product.Variants.Add(new ProductVariant
                {
                    ProductId = product.Id,
                    SKU = vr.SKU,
                    Size = vr.Size,
                    Color = vr.Color,
                    Material = vr.Material,
                    PriceDelta = vr.PriceDelta,
                    CreatedAt = now
                });
            }
        }

        // Record price history
        _context.PriceHistories.Add(new PriceHistory
        {
            ProductId = product.Id,
            OldPrice = 0,
            NewPrice = product.BasePrice,
            ChangedAt = now,
            ChangedByUserId = userId
        });

        await _context.SaveChangesAsync();

        return (await GetProductByIdAsync(product.Id))!;
    }

    public async Task<ProductDetailDto> UpdateProductAsync(Guid productId, UpdateProductRequest request, Guid userId)
    {
        var product = await _context.Products.FindAsync(productId)
            ?? throw new NotFoundException($"Product with ID '{productId}' not found.");

        var now = DateTime.UtcNow;
        var oldPrice = product.BasePrice;

        if (request.Name != null) product.Name = request.Name;
        if (request.Description != null) product.Description = request.Description;
        if (request.BasePrice.HasValue) product.BasePrice = request.BasePrice.Value;
        if (request.Weight.HasValue) product.Weight = request.Weight.Value;
        if (request.CategoryId.HasValue)
        {
            var category = await _context.Categories.FindAsync(request.CategoryId.Value)
                ?? throw new NotFoundException($"Category with ID '{request.CategoryId.Value}' not found.");
            product.CategoryId = request.CategoryId.Value;
        }
        if (request.ImageUrls != null) product.ImageUrls = request.ImageUrls;
        if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;

        product.UpdatedAt = now;

        // Track price changes
        if (request.BasePrice.HasValue && request.BasePrice.Value != oldPrice)
        {
            _context.PriceHistories.Add(new PriceHistory
            {
                ProductId = product.Id,
                OldPrice = oldPrice,
                NewPrice = request.BasePrice.Value,
                ChangedAt = now,
                ChangedByUserId = userId
            });
        }

        await _context.SaveChangesAsync();
        return (await GetProductByIdAsync(product.Id))!;
    }

    public async Task<ProductReviewDto> AddReviewAsync(Guid productId, CreateProductReviewRequest request, Guid userId)
    {
        var product = await _context.Products.FindAsync(productId)
            ?? throw new NotFoundException($"Product with ID '{productId}' not found.");

        if (request.Rating < 1 || request.Rating > 5)
            throw new ValidationException("Rating must be between 1 and 5.");

        // Check if user already reviewed this product
        if (await _context.ProductReviews.AnyAsync(r => r.ProductId == productId && r.UserId == userId))
            throw new ConflictException("You have already reviewed this product.");

        // Check if user has purchased this product (verified purchase)
        var isVerified = await _context.OrderItems
            .AnyAsync(oi => oi.ProductVariant.ProductId == productId
                         && oi.Order.UserId == userId
                         && oi.Order.Status == Models.Enums.OrderStatus.Delivered);

        var user = await _context.AppUsers.FindAsync(userId)
            ?? throw new NotFoundException("User not found.");

        var review = new ProductReview
        {
            ProductId = productId,
            UserId = userId,
            Rating = request.Rating,
            ReviewText = request.ReviewText,
            IsVerifiedPurchase = isVerified,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductReviews.Add(review);
        await _context.SaveChangesAsync();

        return new ProductReviewDto(
            review.Id, review.UserId, user.Username,
            review.Rating, review.ReviewText, review.IsVerifiedPurchase, review.CreatedAt
        );
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.SubCategories)
            .Where(c => c.ParentCategoryId == null) // Root categories only
            .ToListAsync();

        return categories.Select(MapCategory).ToList();
    }

    private static CategoryDto MapCategory(Category c) => new(
        c.Id, c.Name, c.Description, c.ParentCategoryId,
        c.SubCategories.Select(MapCategory).ToList()
    );
}
