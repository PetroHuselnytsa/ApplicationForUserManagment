namespace TestFirstProject.Models
{
    /// <summary>
    /// A line item in an order with price snapshot at time of purchase.
    /// </summary>
    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public Guid ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;

        public string ProductName { get; set; } = null!;
        public string VariantSku { get; set; } = null!;

        /// <summary>
        /// Unit price at the time of purchase (snapshot).
        /// </summary>
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
