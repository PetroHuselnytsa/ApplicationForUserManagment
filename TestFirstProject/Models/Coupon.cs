using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// Coupon code for discounts.
    /// </summary>
    public class Coupon
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public CouponType Type { get; set; }

        /// <summary>
        /// For Percentage: the percent off (0-100). For FixedAmount: the dollar amount off.
        /// </summary>
        public decimal Value { get; set; }

        public decimal? MinOrderValue { get; set; }
        public int? MaxUses { get; set; }
        public int TimesUsed { get; set; }

        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
    }
}
