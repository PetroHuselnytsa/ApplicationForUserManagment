namespace TestFirstProject.Models;

/// <summary>
/// Persistent shopping cart per user. Survives logout.
/// Expires after 30 days of inactivity.
/// </summary>
public class Cart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    /// <summary>
    /// Applied coupon code, if any.
    /// </summary>
    public Guid? AppliedCouponId { get; set; }
    public Coupon? AppliedCoupon { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }

    // Navigation
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
