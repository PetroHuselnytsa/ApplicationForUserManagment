namespace Order.API.DTOs;

/// <summary>
/// Response returned after successfully creating an order.
/// </summary>
public class CreateOrderResponse
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
