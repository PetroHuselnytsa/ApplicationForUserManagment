using TestFirstProject.DTOs;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.Services.Interfaces
{
    public interface IShippingService
    {
        /// <summary>Calculate shipping cost based on weight, method, and zone.</summary>
        Task<decimal> CalculateShippingCostAsync(decimal totalWeight, ShippingMethod method, string? zoneName);

        /// <summary>Calculate estimated delivery date.</summary>
        Task<DateTime> CalculateEstimatedDeliveryAsync(ShippingMethod method, string? zoneName);

        /// <summary>Create a shipment for an order.</summary>
        Task CreateShipmentAsync(Guid orderId, ShippingMethod method, decimal totalWeight, string? zoneName);

        /// <summary>Get tracking info for an order.</summary>
        Task<TrackingResponse> GetTrackingAsync(Guid orderId);
    }
}
