namespace TestFirstProject.Models
{
    /// <summary>
    /// Core product entity with catalog information.
    /// </summary>
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Sku { get; set; } = null!;
        public decimal BasePrice { get; set; }
        public decimal Weight { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Category
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Image URLs stored as JSON array
        public List<string> ImageUrls { get; set; } = new();

        // Navigation
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
        public ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
        public ICollection<FlashSale> FlashSales { get; set; } = new List<FlashSale>();
    }
}
