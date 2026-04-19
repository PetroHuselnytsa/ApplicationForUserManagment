using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models;

/// <summary>
/// Shipment information for an order including tracking.
/// </summary>
public class Shipment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public ShippingMethodType Method { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }

    public decimal ShippingCost { get; set; }
    public decimal WeightKg { get; set; }
    public string DistanceZone { get; set; } = "domestic";

    public DateTime? ShippedAt { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
