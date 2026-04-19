using TestFirstProject.DTOs;
using TestFirstProject.Models;

namespace TestFirstProject.Services.Interfaces
{
    public interface IPromotionService
    {
        /// <summary>Validate and calculate discount for a coupon code against a subtotal.</summary>
        Task<(decimal discount, Coupon? coupon)> CalculateCouponDiscountAsync(string? couponCode, decimal subTotal);

        /// <summary>Record coupon usage after order placement.</summary>
        Task RecordCouponUsageAsync(string couponCode, Guid userId, Guid orderId);

        /// <summary>Get active flash sale price for a product, if any.</summary>
        Task<decimal?> GetFlashSalePriceAsync(Guid productId);

        /// <summary>Batch-fetch active flash sale prices for multiple products in a single query.</summary>
        Task<Dictionary<Guid, decimal>> GetFlashSalePricesAsync(IEnumerable<Guid> productIds);

        /// <summary>Award loyalty points for a purchase. 1 point per dollar spent.</summary>
        Task AwardLoyaltyPointsAsync(Guid userId, decimal orderTotal, Guid orderId);

        /// <summary>Get user's loyalty point balance.</summary>
        Task<LoyaltyBalanceResponse> GetLoyaltyBalanceAsync(Guid userId);

        /// <summary>Redeem loyalty points for a discount. 100 points = $1.</summary>
        Task<decimal> RedeemLoyaltyPointsAsync(Guid userId, int points, Guid orderId);
    }
}
