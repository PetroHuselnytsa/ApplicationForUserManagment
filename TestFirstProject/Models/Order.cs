using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models;

/// <summary>
/// Core order entity containing line items, addresses, and applied discounts.
/// Status transitions are managed exclusively through OrderStateMachine.
/// </summary>
public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    // Addresses (stored as JSON strings for snapshot purposes)
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;

    // Pricing
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }

    // Shipping
    public ShippingMethodType ShippingMethod { get; set; }

    // Coupon
    public Guid? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    // Loyalty points redeemed
    public int LoyaltyPointsUsed { get; set; }
    public decimal LoyaltyDiscount { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Optimistic concurrency token for safe state transitions.
    /// </summary>
    public uint RowVersion { get; set; }

    // Navigation
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Shipment? Shipment { get; set; }
}
