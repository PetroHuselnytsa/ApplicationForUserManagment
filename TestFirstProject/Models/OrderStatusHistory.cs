using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models;

/// <summary>
/// Immutable audit log of order state transitions.
/// Records who changed the status and when.
/// </summary>
public class OrderStatusHistory
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }

    /// <summary>
    /// The user who performed this transition.
    /// </summary>
    public Guid ChangedByUserId { get; set; }

    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; }
}
