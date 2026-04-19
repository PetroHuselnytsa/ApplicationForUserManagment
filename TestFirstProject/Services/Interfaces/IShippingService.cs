using TestFirstProject.DTOs.Orders;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.Services.Interfaces;

public interface IShippingService
{
    /// <summary>
    /// Calculates shipping cost based on total weight and distance zone.
    /// </summary>
    decimal CalculateShippingCost(ShippingMethodType method, decimal totalWeightKg, string distanceZone = "domestic");

    /// <summary>
    /// Calculates estimated delivery date from ship date.
    /// </summary>
    DateTime CalculateEstimatedDelivery(ShippingMethodType method, DateTime shipDate);

    /// <summary>
    /// Creates a shipment record for an order.
    /// </summary>
    Task<ShipmentDto> CreateShipmentAsync(Guid orderId, ShippingMethodType method, decimal weightKg, string distanceZone);

    /// <summary>
    /// Assigns tracking number and carrier to a shipment.
    /// </summary>
    Task<ShipmentDto> AssignTrackingAsync(Guid shipmentId, string trackingNumber, string carrier);
}
