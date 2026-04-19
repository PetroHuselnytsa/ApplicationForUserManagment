using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly PersonsContext _context;
        private readonly IPromotionService _promotionService;

        public ProductService(PersonsContext context, IPromotionService promotionService)
        {
            _context = context;
            _promotionService = promotionService;
        }

        public async Task<PaginatedResponse<ProductListResponse>> GetProductsAsync(
            int page, int pageSize, Guid? categoryId, decimal? minPrice, decimal? maxPrice, bool? inStock, string? search)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            IQueryable<Product> query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.StockEntries)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice <= maxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => EF.Functions.ILike(p.Name, $"%{search}%"));
            }

            if (inStock.HasValue && inStock.Value)
            {
                query = query.Where(p => p.Variants.Any(v =>
                    v.IsActive && v.StockEntries.Any(se =>
                        (se.QuantityOnHand - se.QuantityReserved) > 0)));
            }

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = products.Select(p =>
            {
                double averageRating = p.Reviews.Count > 0
                    ? Math.Round(p.Reviews.Average(r => r.Rating), 2)
                    : 0;

                bool hasStock = p.Variants.Any(v =>
                    v.IsActive && v.StockEntries.Sum(se => se.QuantityOnHand - se.QuantityReserved) > 0);

                return new ProductListResponse(
                    p.Id,
                    p.Name,
                    p.Sku,
                    p.BasePrice,
                    p.IsActive,
                    p.Category.Name,
                    p.ImageUrls,
                    averageRating,
                    hasStock);
            }).ToList();

            return new PaginatedResponse<ProductListResponse>(items, totalCount, page, pageSize, totalPages);
        }

        public async Task<ProductResponse> GetProductByIdAsync(Guid id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.StockEntries)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException($"Product with id '{id}' not found.");

            decimal? flashSalePrice = await _promotionService.GetFlashSalePriceAsync(product.Id);

            var variantResponses = product.Variants.Select(v =>
            {
                int availableStock = v.StockEntries
                    .Sum(se => se.QuantityOnHand - se.QuantityReserved);
                availableStock = Math.Max(0, availableStock);

                decimal finalPrice = flashSalePrice ?? (product.BasePrice + v.PriceDelta);

                return new VariantResponse(
                    v.Id,
                    v.Sku,
                    v.Size,
                    v.Color,
                    v.Material,
                    v.PriceDelta,
                    finalPrice,
                    v.IsActive,
                    availableStock);
            }).ToList();

            double averageRating = product.Reviews.Count > 0
                ? Math.Round(product.Reviews.Average(r => r.Rating), 2)
                : 0;

            return new ProductResponse(
                product.Id,
                product.Name,
                product.Description,
                product.Sku,
                product.BasePrice,
                product.Weight,
                product.IsActive,
                product.CategoryId,
                product.Category.Name,
                product.ImageUrls,
                averageRating,
                product.Reviews.Count,
                variantResponses);
        }

        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, Guid userId)
        {
            bool skuExists = await _context.Products
                .AnyAsync(p => p.Sku == request.Sku);

            if (skuExists)
            {
                throw new ConflictException($"A product with SKU '{request.Sku}' already exists.");
            }

            if (request.Variants != null && request.Variants.Count > 0)
            {
                var variantSkus = request.Variants.Select(v => v.Sku).ToList();
                var duplicateVariantSkus = variantSkus
                    .GroupBy(s => s)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateVariantSkus.Count > 0)
                {
                    throw new ValidationException(
                        $"Duplicate variant SKUs in request: {string.Join(", ", duplicateVariantSkus)}");
                }

                bool variantSkuExists = await _context.ProductVariants
                    .AnyAsync(v => variantSkus.Contains(v.Sku));

                if (variantSkuExists)
                {
                    throw new ConflictException("One or more variant SKUs already exist.");
                }
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Sku = request.Sku,
                BasePrice = request.BasePrice,
                Weight = request.Weight,
                CategoryId = request.CategoryId,
                ImageUrls = request.ImageUrls ?? new List<string>(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (request.Variants != null)
            {
                foreach (var variantRequest in request.Variants)
                {
                    product.Variants.Add(new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Sku = variantRequest.Sku,
                        Size = variantRequest.Size,
                        Color = variantRequest.Color,
                        Material = variantRequest.Material,
                        PriceDelta = variantRequest.PriceDelta,
                        IsActive = true
                    });
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return await GetProductByIdAsync(product.Id);
        }

        public async Task<ProductResponse> UpdateProductAsync(Guid id, UpdateProductRequest request, Guid userId)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException($"Product with id '{id}' not found.");

            if (request.BasePrice.HasValue && request.BasePrice.Value != product.BasePrice)
            {
                var priceHistory = new PriceHistory
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    OldPrice = product.BasePrice,
                    NewPrice = request.BasePrice.Value,
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = userId.ToString()
                };

                _context.PriceHistories.Add(priceHistory);
                product.BasePrice = request.BasePrice.Value;
            }

            if (request.Name != null)
            {
                product.Name = request.Name;
            }

            if (request.Description != null)
            {
                product.Description = request.Description;
            }

            if (request.Weight.HasValue)
            {
                product.Weight = request.Weight.Value;
            }

            if (request.CategoryId.HasValue)
            {
                product.CategoryId = request.CategoryId.Value;
            }

            if (request.ImageUrls != null)
            {
                product.ImageUrls = request.ImageUrls;
            }

            if (request.IsActive.HasValue)
            {
                product.IsActive = request.IsActive.Value;
            }

            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetProductByIdAsync(product.Id);
        }

        public async Task<ReviewResponse> AddReviewAsync(Guid productId, CreateReviewRequest request, Guid userId)
        {
            if (request.Rating < 1 || request.Rating > 5)
            {
                throw new ValidationException("Rating must be between 1 and 5.");
            }

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId)
                ?? throw new NotFoundException($"Product with id '{productId}' not found.");

            bool alreadyReviewed = await _context.ProductReviews
                .AnyAsync(r => r.ProductId == productId && r.UserId == userId);

            if (alreadyReviewed)
            {
                throw new ConflictException("You have already reviewed this product.");
            }

            var user = await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new NotFoundException("User not found.");

            bool isVerifiedPurchase = await _context.OrderItems
                .AnyAsync(oi => oi.ProductVariant.ProductId == productId
                    && oi.Order.UserId == userId);

            var review = new ProductReview
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                UserId = userId,
                Rating = request.Rating,
                Text = request.Text,
                IsVerifiedPurchase = isVerifiedPurchase,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            return new ReviewResponse(
                review.Id,
                user.Username,
                review.Rating,
                review.Text,
                review.IsVerifiedPurchase,
                review.CreatedAt);
        }
    }
}
