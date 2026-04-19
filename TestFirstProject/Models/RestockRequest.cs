using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// A request created by a manager to restock a product variant at a warehouse.
    /// </summary>
    public class RestockRequest
    {
        public Guid Id { get; set; }
        public Guid ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;

        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public int RequestedQuantity { get; set; }
        public RestockRequestStatus Status { get; set; } = RestockRequestStatus.Pending;

        public Guid RequestedByUserId { get; set; }
        public AppUser RequestedByUser { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FulfilledAt { get; set; }
    }
}
