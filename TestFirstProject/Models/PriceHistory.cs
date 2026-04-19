namespace TestFirstProject.Models;

/// <summary>
/// Tracks historical price changes for products.
/// </summary>
public class PriceHistory
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ChangedAt { get; set; }
    public Guid? ChangedByUserId { get; set; }
}
