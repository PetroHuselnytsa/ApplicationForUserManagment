namespace TestFirstProject.Models;

/// <summary>
/// Product review with rating and verified purchase tracking.
/// </summary>
public class ProductReview
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public int Rating { get; set; } // 1-5
    public string? ReviewText { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public DateTime CreatedAt { get; set; }
}
