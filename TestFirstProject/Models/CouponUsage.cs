namespace TestFirstProject.Models
{
    /// <summary>
    /// Records each use of a coupon.
    /// </summary>
    public class CouponUsage
    {
        public Guid Id { get; set; }
        public Guid CouponId { get; set; }
        public Coupon Coupon { get; set; } = null!;

        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    }
}
