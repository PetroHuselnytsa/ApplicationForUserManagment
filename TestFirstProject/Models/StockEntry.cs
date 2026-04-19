namespace TestFirstProject.Models
{
    /// <summary>
    /// Stock level for a specific product variant in a specific warehouse.
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
        /// When available stock (OnHand - Reserved) drops below this, trigger a low-stock alert.
        /// </summary>
        public int LowStockThreshold { get; set; } = 10;
    }
}
