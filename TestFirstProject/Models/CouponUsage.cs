namespace TestFirstProject.Models;

/// <summary>
/// Tracks individual coupon usage per user per order.
/// </summary>
public class CouponUsage
{
    public Guid Id { get; set; }
    public Guid CouponId { get; set; }
    public Coupon Coupon { get; set; } = null!;

    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public DateTime UsedAt { get; set; }
}
