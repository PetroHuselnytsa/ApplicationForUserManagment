namespace TestFirstProject.Models
{
    /// <summary>
    /// A specific variant of a product (e.g. size/color/material combination).
    /// Each variant has its own SKU and price delta.
    /// </summary>
    public class ProductVariant
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string Sku { get; set; } = null!;
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Material { get; set; }

        /// <summary>
        /// Added to the product's BasePrice to get the final variant price.
        /// Can be negative for cheaper variants.
        /// </summary>
        public decimal PriceDelta { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<StockEntry> StockEntries { get; set; } = new List<StockEntry>();
    }
}
