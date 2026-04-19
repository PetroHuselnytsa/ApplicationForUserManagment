using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// Core order entity with full lifecycle support.
    /// </summary>
    public class Order
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = null!;

        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public OrderStatus Status { get; set; } = OrderStatus.Draft;

        // Addresses
        public string ShippingAddress { get; set; } = null!;
        public string BillingAddress { get; set; } = null!;

        // Totals (snapshot at order creation)
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }

        public string? CouponCode { get; set; }
        public ShippingMethod ShippingMethod { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public Shipment? Shipment { get; set; }
    }
}
