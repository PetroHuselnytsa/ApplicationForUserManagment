namespace TestFirstProject.Models;

/// <summary>
/// Time-limited price reduction on a specific product.
/// </summary>
public class FlashSale
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Discounted price during the flash sale period.
    /// </summary>
    public decimal SalePrice { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
