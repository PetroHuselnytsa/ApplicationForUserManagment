namespace TestFirstProject.Models
{
    /// <summary>
    /// Persistent shopping cart per user. Survives logout.
    /// </summary>
    public class Cart
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Applied coupon code, if any.
        /// </summary>
        public string? CouponCode { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
