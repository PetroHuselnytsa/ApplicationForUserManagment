using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// Shipping information for an order.
    /// </summary>
    public class Shipment
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public ShippingMethod Method { get; set; }
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;

        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }

        public decimal ShippingCost { get; set; }
        public decimal TotalWeight { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
