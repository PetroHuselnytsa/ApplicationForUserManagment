namespace TestFirstProject.Models;

/// <summary>
/// Product variant with specific attributes (size, color, material).
/// Each variant has its own SKU and price delta from the base product.
/// </summary>
public class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string SKU { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? Material { get; set; }

    /// <summary>
    /// Price adjustment from the base product price. Can be positive or negative.
    /// Final price = Product.BasePrice + PriceDelta
    /// </summary>
    public decimal PriceDelta { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<StockEntry> StockEntries { get; set; } = new List<StockEntry>();
}
