namespace TestFirstProject.Models;

/// <summary>
/// Individual item in a shopping cart, referencing a specific product variant.
/// </summary>
public class CartItem
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; }
}
