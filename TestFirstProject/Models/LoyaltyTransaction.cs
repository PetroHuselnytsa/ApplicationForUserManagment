using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// Tracks loyalty point earnings and redemptions.
    /// </summary>
    public class LoyaltyTransaction
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public LoyaltyTransactionType Type { get; set; }
        public int Points { get; set; }
        public string? Description { get; set; }

        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
