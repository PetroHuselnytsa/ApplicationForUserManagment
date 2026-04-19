namespace TestFirstProject.Models;

/// <summary>
/// Stock level per product variant per warehouse.
/// Tracks both available and reserved quantities.
/// </summary>
public class StockEntry
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }

    /// <summary>
    /// Low stock alert threshold, configurable per product per warehouse.
    /// </summary>
    public int LowStockThreshold { get; set; } = 10;

    public DateTime LastUpdated { get; set; }
}
