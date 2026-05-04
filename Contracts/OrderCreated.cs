namespace Contracts;

/// <summary>
/// Event published by Order.API when a new order is successfully created.
/// Consumed by Notification.Service (and any other interested consumers).
/// </summary>
/// <param name="OrderId">Unique identifier of the newly created order.</param>
/// <param name="CustomerId">Identifier of the customer who placed the order.</param>
/// <param name="TotalAmount">Monetary total of the order.</param>
/// <param name="CreatedAt">UTC timestamp of order creation.</param>
public record OrderCreated(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    DateTime CreatedAt);
