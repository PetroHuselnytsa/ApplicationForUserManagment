using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models;

/// <summary>
/// Discount coupon with usage limits and expiry.
/// </summary>
public class Coupon
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public CouponType Type { get; set; }

    /// <summary>
    /// Discount value: percentage (0-100) or fixed amount.
    /// </summary>
    public decimal DiscountValue { get; set; }

    public decimal? MinOrderValue { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }

    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
}
