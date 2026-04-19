namespace TestFirstProject.DTOs.Cart;

// --- Request DTOs ---

public record AddCartItemRequest(
    Guid ProductVariantId,
    int Quantity
);

public record UpdateCartItemRequest(
    int Quantity
);

public record ApplyCouponRequest(
    string CouponCode
);

public record CheckoutRequest(
    string ShippingAddress,
    string BillingAddress,
    string ShippingMethod,  // Standard, Express, SameDay, Pickup
    int LoyaltyPointsToRedeem = 0,
    string? Notes = null
);

// --- Response DTOs ---

public record CartDto(
    Guid Id,
    List<CartItemDto> Items,
    string? AppliedCouponCode,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal EstimatedTotal,
    DateTime LastActivityAt
);

public record CartItemDto(
    Guid Id,
    Guid ProductVariantId,
    string ProductName,
    string VariantDescription,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice,
    string? ImageUrl
);
