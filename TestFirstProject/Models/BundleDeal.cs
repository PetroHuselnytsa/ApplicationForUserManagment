namespace TestFirstProject.Models
{
    /// <summary>
    /// Buy X get Y bundle deal.
    /// </summary>
    public class BundleDeal
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public Guid BuyProductId { get; set; }
        public Product BuyProduct { get; set; } = null!;
        public int BuyQuantity { get; set; }

        public Guid GetProductId { get; set; }
        public Product GetProduct { get; set; } = null!;
        public int GetQuantity { get; set; }

        /// <summary>
        /// Discount percentage on the "get" product (100 = free).
        /// </summary>
        public decimal GetDiscountPercent { get; set; } = 100m;

        public bool IsActive { get; set; } = true;
        public DateTime StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
    }
}
