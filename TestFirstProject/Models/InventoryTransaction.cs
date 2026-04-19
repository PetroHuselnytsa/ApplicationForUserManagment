using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models;

/// <summary>
/// Immutable log of every inventory change with reason tracking.
/// </summary>
public class InventoryTransaction
{
    public Guid Id { get; set; }
    public Guid StockEntryId { get; set; }
    public StockEntry StockEntry { get; set; } = null!;

    public InventoryTransactionType Type { get; set; }
    public int QuantityChange { get; set; }
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Optional reference to the order that triggered this transaction.
    /// </summary>
    public Guid? OrderId { get; set; }

    public Guid? PerformedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
