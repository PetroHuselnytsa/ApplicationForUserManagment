using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// Audit log for every inventory change.
    /// </summary>
    public class InventoryTransaction
    {
        public Guid Id { get; set; }
        public Guid StockEntryId { get; set; }
        public StockEntry StockEntry { get; set; } = null!;

        public InventoryTransactionType Type { get; set; }
        public int QuantityChange { get; set; }
        public string? Reason { get; set; }
        public string? ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
