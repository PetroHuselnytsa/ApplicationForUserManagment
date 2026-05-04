namespace Contracts;

/// <summary>
/// Event published when a new order is created.
/// Consumed by Notification.Service to log order details.
/// </summary>
public record OrderCreated
{
    public Guid OrderId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public DateTime CreatedAt { get; init; }
}
