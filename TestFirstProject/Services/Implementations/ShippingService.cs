using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class ShippingService : IShippingService
    {
        private readonly PersonsContext _context;

        public ShippingService(PersonsContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculateShippingCostAsync(decimal totalWeight, ShippingMethod method, string? zoneName)
        {
            if (method == ShippingMethod.Pickup)
                return 0;

            var zone = await GetShippingZoneAsync(zoneName);
            return CalculateCost(zone, totalWeight, method);
        }

        public async Task<DateTime> CalculateEstimatedDeliveryAsync(ShippingMethod method, string? zoneName)
        {
            var zone = await GetShippingZoneAsync(zoneName);
            return DateTime.UtcNow.AddDays(GetDeliveryDays(zone, method));
        }

        public async Task CreateShipmentAsync(Guid orderId, ShippingMethod method, decimal totalWeight, string? zoneName)
        {
            var zone = await GetShippingZoneAsync(zoneName);
            var cost = CalculateCost(zone, totalWeight, method);
            var estimatedDelivery = DateTime.UtcNow.AddDays(GetDeliveryDays(zone, method));

            string? carrier = method switch
            {
                ShippingMethod.Standard => "StandardPost",
                ShippingMethod.Express => "ExpressLogistics",
                ShippingMethod.SameDay => "SameDayDelivery",
                _ => null
            };

            var shipment = new Shipment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Method = method,
                Status = ShipmentStatus.Pending,
                TrackingNumber = $"TRK-{Guid.NewGuid().ToString("N")[..12].ToUpper()}",
                Carrier = carrier,
                ShippingCost = cost,
                TotalWeight = totalWeight,
                EstimatedDeliveryDate = estimatedDelivery,
                CreatedAt = DateTime.UtcNow
            };

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();
        }

        public async Task<TrackingResponse> GetTrackingAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new NotFoundException("Order not found.");

            return new TrackingResponse(
                order.Id,
                order.OrderNumber,
                order.Status.ToString(),
                order.Shipment?.Status.ToString(),
                order.Shipment?.TrackingNumber,
                order.Shipment?.Carrier,
                order.Shipment?.EstimatedDeliveryDate,
                order.Shipment?.ShippedAt,
                order.Shipment?.DeliveredAt);
        }

        // --- Private helpers ---

        private static decimal CalculateCost(ShippingZone zone, decimal totalWeight, ShippingMethod method)
        {
            if (method == ShippingMethod.Pickup)
                return 0;

            decimal cost = zone.BaseCost + (totalWeight > 1 ? (totalWeight - 1) * zone.CostPerKg : 0);

            return method switch
            {
                ShippingMethod.Express => cost * zone.ExpressMultiplier,
                ShippingMethod.SameDay => cost * zone.SameDayMultiplier,
                _ => cost
            };
        }

        private static int GetDeliveryDays(ShippingZone zone, ShippingMethod method)
        {
            return method switch
            {
                ShippingMethod.Standard => zone.StandardDeliveryDays,
                ShippingMethod.Express => zone.ExpressDeliveryDays,
                ShippingMethod.SameDay => 1,
                ShippingMethod.Pickup => 0,
                _ => zone.StandardDeliveryDays
            };
        }

        private async Task<ShippingZone> GetShippingZoneAsync(string? zoneName)
        {
            ShippingZone? zone = null;

            if (!string.IsNullOrEmpty(zoneName))
            {
                zone = await _context.ShippingZones
                    .FirstOrDefaultAsync(z => z.Name == zoneName);
            }

            zone ??= await _context.ShippingZones
                .FirstOrDefaultAsync(z => z.Name == "Regional");

            return zone ?? throw new NotFoundException("Shipping zone not found.");
        }
    }
}
