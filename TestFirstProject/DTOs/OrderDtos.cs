using TestFirstProject.Models.Enums;

namespace TestFirstProject.DTOs
{
    // --- Orders ---

    public record OrderResponse(
        Guid Id,
        string OrderNumber,
        string Status,
        string ShippingAddress,
        string BillingAddress,
        decimal SubTotal,
        decimal DiscountAmount,
        decimal ShippingCost,
        decimal TotalAmount,
        string? CouponCode,
        string ShippingMethod,
        DateTime CreatedAt,
        List<OrderItemResponse> Items,
        List<OrderStatusHistoryResponse> StatusHistory);

    public record OrderItemResponse(
        Guid Id,
        string ProductName,
        string VariantSku,
        decimal UnitPrice,
        int Quantity,
        decimal LineTotal);

    public record OrderStatusHistoryResponse(
        string FromStatus,
        string ToStatus,
        string ChangedBy,
        string? Notes,
        DateTime ChangedAt);

    public record OrderListResponse(
        Guid Id,
        string OrderNumber,
        string Status,
        decimal TotalAmount,
        int ItemCount,
        DateTime CreatedAt);

    public record UpdateOrderStatusRequest(
        string Status,
        string? Notes);

    public record TrackingResponse(
        Guid OrderId,
        string OrderNumber,
        string OrderStatus,
        string? ShipmentStatus,
        string? TrackingNumber,
        string? Carrier,
        DateTime? EstimatedDeliveryDate,
        DateTime? ShippedAt,
        DateTime? DeliveredAt);
}
