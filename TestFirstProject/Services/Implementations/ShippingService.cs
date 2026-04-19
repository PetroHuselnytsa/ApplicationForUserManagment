using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Orders;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations;

public class ShippingService : IShippingService
{
    private readonly PersonsContext _context;

    // Base rates per shipping method (flat rate in dollars)
    private static readonly Dictionary<ShippingMethodType, decimal> BaseRates = new()
    {
        [ShippingMethodType.Standard] = 5.99m,
        [ShippingMethodType.Express] = 14.99m,
        [ShippingMethodType.SameDay] = 24.99m,
        [ShippingMethodType.Pickup] = 0m
    };

    // Weight surcharge per kg
    private static readonly Dictionary<ShippingMethodType, decimal> WeightRatePerKg = new()
    {
        [ShippingMethodType.Standard] = 0.50m,
        [ShippingMethodType.Express] = 1.00m,
        [ShippingMethodType.SameDay] = 1.50m,
        [ShippingMethodType.Pickup] = 0m
    };

    // Zone multipliers
    private static readonly Dictionary<string, decimal> ZoneMultipliers = new()
    {
        ["local"] = 0.8m,
        ["domestic"] = 1.0m,
        ["regional"] = 1.5m,
        ["international"] = 2.5m
    };

    // Delivery days per method
    private static readonly Dictionary<ShippingMethodType, int> DeliveryDays = new()
    {
        [ShippingMethodType.Standard] = 7,
        [ShippingMethodType.Express] = 3,
        [ShippingMethodType.SameDay] = 0,
        [ShippingMethodType.Pickup] = 0
    };

    public ShippingService(PersonsContext context)
    {
        _context = context;
    }

    public decimal CalculateShippingCost(ShippingMethodType method, decimal totalWeightKg, string distanceZone = "domestic")
    {
        var baseRate = BaseRates[method];
        var weightCharge = WeightRatePerKg[method] * totalWeightKg;
        var zoneMultiplier = ZoneMultipliers.GetValueOrDefault(distanceZone.ToLower(), 1.0m);

        return Math.Round((baseRate + weightCharge) * zoneMultiplier, 2);
    }

    public DateTime CalculateEstimatedDelivery(ShippingMethodType method, DateTime shipDate)
    {
        var days = DeliveryDays[method];
        // Skip weekends for non-same-day shipping
        if (days == 0) return shipDate;

        var deliveryDate = shipDate;
        var addedDays = 0;
        while (addedDays < days)
        {
            deliveryDate = deliveryDate.AddDays(1);
            if (deliveryDate.DayOfWeek != DayOfWeek.Saturday && deliveryDate.DayOfWeek != DayOfWeek.Sunday)
                addedDays++;
        }
        return deliveryDate;
    }

    public async Task<ShipmentDto> CreateShipmentAsync(Guid orderId, ShippingMethodType method, decimal weightKg, string distanceZone)
    {
        var order = await _context.Orders.FindAsync(orderId)
            ?? throw new NotFoundException($"Order '{orderId}' not found.");

        var cost = CalculateShippingCost(method, weightKg, distanceZone);
        var now = DateTime.UtcNow;

        var shipment = new Shipment
        {
            OrderId = orderId,
            Method = method,
            ShippingCost = cost,
            WeightKg = weightKg,
            DistanceZone = distanceZone,
            CreatedAt = now
        };

        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync();

        return MapToDto(shipment);
    }

    public async Task<ShipmentDto> AssignTrackingAsync(Guid shipmentId, string trackingNumber, string carrier)
    {
        var shipment = await _context.Shipments.FindAsync(shipmentId)
            ?? throw new NotFoundException($"Shipment '{shipmentId}' not found.");

        shipment.TrackingNumber = trackingNumber;
        shipment.Carrier = carrier;
        shipment.ShippedAt = DateTime.UtcNow;
        shipment.EstimatedDeliveryDate = CalculateEstimatedDelivery(shipment.Method, DateTime.UtcNow);

        await _context.SaveChangesAsync();

        return MapToDto(shipment);
    }

    private static ShipmentDto MapToDto(Shipment s) => new(
        s.Id, s.Method.ToString(), s.TrackingNumber, s.Carrier,
        s.ShippingCost, s.ShippedAt, s.EstimatedDeliveryDate, s.DeliveredAt
    );
}
