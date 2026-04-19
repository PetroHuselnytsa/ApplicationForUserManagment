using TestFirstProject.Models.Enums;

namespace TestFirstProject.DTOs.Orders;

// --- Request DTOs ---

public record UpdateOrderStatusRequest(
    string NewStatus,
    string? Notes
);

// --- Response DTOs ---

public record OrderListDto(
    Guid Id,
    OrderStatus Status,
    decimal TotalAmount,
    int ItemCount,
    ShippingMethodType ShippingMethod,
    DateTime CreatedAt
);

public record OrderDetailDto(
    Guid Id,
    OrderStatus Status,
    string ShippingAddress,
    string BillingAddress,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal ShippingCost,
    decimal TotalAmount,
    ShippingMethodType ShippingMethod,
    int LoyaltyPointsUsed,
    decimal LoyaltyDiscount,
    string? CouponCode,
    string? Notes,
    DateTime CreatedAt,
    List<OrderItemDto> Items,
    List<OrderStatusHistoryDto> StatusHistory
);

public record OrderItemDto(
    Guid Id,
    Guid ProductVariantId,
    string ProductName,
    string VariantDescription,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);

public record OrderStatusHistoryDto(
    OrderStatus FromStatus,
    OrderStatus ToStatus,
    Guid ChangedByUserId,
    string? Notes,
    DateTime ChangedAt
);

public record OrderTrackingDto(
    Guid OrderId,
    OrderStatus OrderStatus,
    ShipmentDto? Shipment
);

public record ShipmentDto(
    Guid Id,
    string Method,
    string? TrackingNumber,
    string? Carrier,
    decimal ShippingCost,
    DateTime? ShippedAt,
    DateTime? EstimatedDeliveryDate,
    DateTime? DeliveredAt
);

// Admin
public record AdminOrderFilterRequest(
    OrderStatus? Status = null,
    Guid? UserId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20
);

public record SalesReportDto(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    int TotalOrders,
    decimal TotalRevenue,
    decimal TotalDiscounts,
    int ItemsSold,
    decimal AverageOrderValue
);
