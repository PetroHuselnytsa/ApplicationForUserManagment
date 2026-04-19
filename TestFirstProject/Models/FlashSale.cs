namespace TestFirstProject.Models
{
    /// <summary>
    /// Time-limited price reduction on a specific product.
    /// </summary>
    public class FlashSale
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        /// <summary>
        /// The sale price during the flash sale period.
        /// </summary>
        public decimal SalePrice { get; set; }

        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
