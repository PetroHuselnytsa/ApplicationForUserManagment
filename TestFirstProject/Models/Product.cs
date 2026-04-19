namespace TestFirstProject.Models;

/// <summary>
/// Core product entity with catalog information.
/// </summary>
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal Weight { get; set; }

    // Category relationship
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Images stored as comma-separated URLs or JSON array
    public string ImageUrls { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    public ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
    public ICollection<FlashSale> FlashSales { get; set; } = new List<FlashSale>();
}
