namespace TestFirstProject.DTOs
{
    // --- Shopping Cart ---

    public record AddCartItemRequest(
        Guid ProductVariantId,
        int Quantity);

    public record UpdateCartItemRequest(
        int Quantity);

    public record ApplyCouponRequest(
        string CouponCode);

    public record CheckoutRequest(
        string ShippingAddress,
        string BillingAddress,
        string ShippingMethod,
        string? ShippingZone);

    public record CartResponse(
        Guid Id,
        List<CartItemResponse> Items,
        string? CouponCode,
        decimal SubTotal,
        decimal DiscountAmount,
        decimal Total,
        DateTime LastActivityAt);

    public record CartItemResponse(
        Guid Id,
        Guid ProductVariantId,
        string ProductName,
        string VariantSku,
        string? Size,
        string? Color,
        decimal UnitPrice,
        int Quantity,
        decimal LineTotal);
}
