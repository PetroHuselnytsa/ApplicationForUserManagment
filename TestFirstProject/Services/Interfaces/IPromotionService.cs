using TestFirstProject.Models;

namespace TestFirstProject.Services.Interfaces;

public interface IPromotionService
{
    /// <summary>
    /// Validates a coupon code and returns the coupon if valid.
    /// </summary>
    Task<Coupon?> ValidateCouponAsync(string code, decimal orderTotal, Guid userId);

    /// <summary>
    /// Calculates the discount amount for a coupon applied to a given subtotal.
    /// </summary>
    decimal CalculateCouponDiscount(Coupon coupon, decimal subtotal);

    /// <summary>
    /// Records coupon usage after a successful order.
    /// </summary>
    Task RecordCouponUsageAsync(Guid couponId, Guid userId, Guid orderId);

    /// <summary>
    /// Gets the active flash sale price for a product, if any.
    /// </summary>
    Task<decimal?> GetFlashSalePriceAsync(Guid productId);

    /// <summary>
    /// Gets a user's loyalty point balance.
    /// </summary>
    Task<int> GetLoyaltyPointsAsync(Guid userId);

    /// <summary>
    /// Awards loyalty points for a purchase (1 point per $1 spent).
    /// </summary>
    Task AwardLoyaltyPointsAsync(Guid userId, decimal orderTotal, Guid orderId);

    /// <summary>
    /// Redeems loyalty points for a discount (100 points = $1).
    /// Returns the actual discount amount applied.
    /// </summary>
    Task<decimal> RedeemLoyaltyPointsAsync(Guid userId, int points, Guid orderId);
}
