namespace TestFirstProject.Models;

/// <summary>
/// Loyalty points earned or redeemed.
/// Earn: 1 point per $1 spent. Redeem: 100 points = $1 discount.
/// </summary>
public class LoyaltyTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    /// <summary>
    /// Positive = earned, Negative = redeemed.
    /// </summary>
    public int Points { get; set; }

    public string Description { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}
