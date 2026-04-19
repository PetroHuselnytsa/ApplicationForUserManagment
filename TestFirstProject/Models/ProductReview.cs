namespace TestFirstProject.Models
{
    /// <summary>
    /// User review for a product.
    /// </summary>
    public class ProductReview
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public int Rating { get; set; }
        public string? Text { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
