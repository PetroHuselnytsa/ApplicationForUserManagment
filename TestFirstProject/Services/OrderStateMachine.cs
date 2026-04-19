using TestFirstProject.Models.Enums;

namespace TestFirstProject.Services;

/// <summary>
/// Dedicated state machine for order status transitions.
/// All order state changes MUST go through this class — never set status directly.
/// Validates that each transition is legal before allowing it.
/// </summary>
public static class OrderStateMachine
{
    /// <summary>
    /// Defines valid transitions: key = current state, value = set of allowed next states.
    /// </summary>
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> ValidTransitions = new()
    {
        [OrderStatus.Draft] = new HashSet<OrderStatus>
        {
            OrderStatus.Pending,
            OrderStatus.Cancelled
        },
        [OrderStatus.Pending] = new HashSet<OrderStatus>
        {
            OrderStatus.Confirmed,
            OrderStatus.Cancelled
        },
        [OrderStatus.Confirmed] = new HashSet<OrderStatus>
        {
            OrderStatus.Processing,
            OrderStatus.Cancelled
        },
        [OrderStatus.Processing] = new HashSet<OrderStatus>
        {
            OrderStatus.Shipped,
            OrderStatus.Cancelled
        },
        [OrderStatus.Shipped] = new HashSet<OrderStatus>
        {
            OrderStatus.Delivered,
            OrderStatus.Cancelled
        },
        [OrderStatus.Delivered] = new HashSet<OrderStatus>
        {
            OrderStatus.Refunded
        },
        [OrderStatus.Cancelled] = new HashSet<OrderStatus>
        {
            OrderStatus.Refunded
        },
        [OrderStatus.Refunded] = new HashSet<OrderStatus>()  // Terminal state
    };

    /// <summary>
    /// Returns true if the transition from currentStatus to newStatus is valid.
    /// </summary>
    public static bool CanTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return ValidTransitions.TryGetValue(currentStatus, out var allowed) && allowed.Contains(newStatus);
    }

    /// <summary>
    /// Validates the transition and throws if invalid.
    /// </summary>
    public static void ValidateTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        if (!CanTransition(currentStatus, newStatus))
        {
            throw new InvalidOperationException(
                $"Invalid order status transition from '{currentStatus}' to '{newStatus}'. " +
                $"Allowed transitions from '{currentStatus}': [{string.Join(", ", GetAllowedTransitions(currentStatus))}]");
        }
    }

    /// <summary>
    /// Gets all valid next states from the current status.
    /// </summary>
    public static IReadOnlySet<OrderStatus> GetAllowedTransitions(OrderStatus currentStatus)
    {
        return ValidTransitions.TryGetValue(currentStatus, out var allowed)
            ? allowed
            : new HashSet<OrderStatus>();
    }
}
