namespace TestFirstProject.Models;

/// <summary>
/// Line item in an order with price snapshot at time of purchase.
/// </summary>
public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    /// <summary>
    /// Snapshot of product name at time of order (denormalized).
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    public string VariantDescription { get; set; } = string.Empty;

    public int Quantity { get; set; }

    /// <summary>
    /// Unit price snapshot at time of order, including variant delta.
    /// </summary>
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
