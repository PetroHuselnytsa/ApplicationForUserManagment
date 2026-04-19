namespace TestFirstProject.Models
{
    /// <summary>
    /// User wallet for balance-based payments.
    /// </summary>
    public class Wallet
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public decimal Balance { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
