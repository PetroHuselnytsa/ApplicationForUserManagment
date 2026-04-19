namespace TestFirstProject.Models
{
    /// <summary>
    /// A single line item in a shopping cart.
    /// </summary>
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        public Guid ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;

        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
