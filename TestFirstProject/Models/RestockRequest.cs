using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models;

/// <summary>
/// Manager-created restock request tracking fulfillment.
/// </summary>
public class RestockRequest
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public int RequestedQuantity { get; set; }
    public int FulfilledQuantity { get; set; }

    public RestockRequestStatus Status { get; set; } = RestockRequestStatus.Pending;

    public Guid RequestedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FulfilledAt { get; set; }
}
