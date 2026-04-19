namespace TestFirstProject.Models.Enums;

/// <summary>
/// Full order lifecycle states.
/// Transitions: Draft → Pending → Confirmed → Processing → Shipped → Delivered
///              Any (except Delivered/Refunded) → Cancelled
///              Delivered/Cancelled → Refunded
/// </summary>
public enum OrderStatus
{
    Draft = 0,
    Pending = 1,
    Confirmed = 2,
    Processing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
    Refunded = 7
}
