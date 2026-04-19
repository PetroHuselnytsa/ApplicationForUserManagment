using TestFirstProject.Exceptions;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Validates and enforces order state transitions.
    /// Never set order status directly — always go through this class.
    /// </summary>
    public static class OrderStateMachine
    {
        // Defines all valid transitions: from-state → set of allowed to-states
        private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> ValidTransitions = new()
        {
            [OrderStatus.Draft] = new() { OrderStatus.Pending, OrderStatus.Cancelled },
            [OrderStatus.Pending] = new() { OrderStatus.Confirmed, OrderStatus.Cancelled },
            [OrderStatus.Confirmed] = new() { OrderStatus.Processing, OrderStatus.Cancelled },
            [OrderStatus.Processing] = new() { OrderStatus.Shipped, OrderStatus.Cancelled },
            [OrderStatus.Shipped] = new() { OrderStatus.Delivered },
            [OrderStatus.Delivered] = new() { OrderStatus.Refunded },
            [OrderStatus.Cancelled] = new(),   // Terminal state
            [OrderStatus.Refunded] = new()     // Terminal state
        };

        /// <summary>
        /// Returns true if transitioning from <paramref name="from"/> to <paramref name="to"/> is valid.
        /// </summary>
        public static bool CanTransition(OrderStatus from, OrderStatus to)
        {
            return ValidTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
        }

        /// <summary>
        /// Validates the transition and throws if invalid.
        /// </summary>
        public static void ValidateTransition(OrderStatus from, OrderStatus to)
        {
            if (!CanTransition(from, to))
            {
                throw new AppException(
                    $"Invalid order state transition: cannot move from '{from}' to '{to}'.",
                    400);
            }
        }

        /// <summary>
        /// Returns all states an order can transition to from its current state.
        /// </summary>
        public static IReadOnlySet<OrderStatus> GetAllowedTransitions(OrderStatus current)
        {
            return ValidTransitions.TryGetValue(current, out var allowed) ? allowed : new HashSet<OrderStatus>();
        }
    }
}
