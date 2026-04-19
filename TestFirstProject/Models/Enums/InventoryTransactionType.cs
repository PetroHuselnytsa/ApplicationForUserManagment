namespace TestFirstProject.Models.Enums
{
    /// <summary>
    /// Reason for an inventory change.
    /// </summary>
    public enum InventoryTransactionType
    {
        Restock = 0,
        Reserved = 1,
        Released = 2,
        Sold = 3,
        Adjustment = 4,
        Returned = 5
    }
}
