namespace TestFirstProject.Models
{
    /// <summary>
    /// Tracks historical price changes for a product.
    /// </summary>
    public class PriceHistory
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? ChangedBy { get; set; }
    }
}
