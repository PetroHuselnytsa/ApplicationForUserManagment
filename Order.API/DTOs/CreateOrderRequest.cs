using System.ComponentModel.DataAnnotations;

namespace Order.API.DTOs;

/// <summary>
/// Request body for creating a new order.
/// </summary>
public class CreateOrderRequest
{
    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }
}
