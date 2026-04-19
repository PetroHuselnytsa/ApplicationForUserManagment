namespace TestFirstProject.Models.Enums;

/// <summary>
/// Types of inventory stock changes.
/// </summary>
public enum InventoryTransactionType
{
    Restock = 0,
    Reservation = 1,
    ReservationRelease = 2,
    Sale = 3,
    Adjustment = 4,
    Return = 5
}
