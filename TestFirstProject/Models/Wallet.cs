namespace TestFirstProject.Models;

/// <summary>
/// User wallet for balance-based payments.
/// Users can top up and pay from wallet balance.
/// </summary>
public class Wallet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
