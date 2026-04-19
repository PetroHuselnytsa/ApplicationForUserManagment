using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class PromotionService : IPromotionService
    {
        private readonly PersonsContext _context;

        public PromotionService(PersonsContext context)
        {
            _context = context;
        }

        public async Task<(decimal discount, Coupon? coupon)> CalculateCouponDiscountAsync(
            string? couponCode, decimal subTotal)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
                return (0, null);

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == couponCode)
                ?? throw new NotFoundException($"Coupon '{couponCode}' not found.");

            if (!coupon.IsActive)
                throw new ValidationException("This coupon is no longer active.");

            if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt.Value < DateTime.UtcNow)
                throw new ValidationException("This coupon has expired.");

            if (coupon.MaxUses.HasValue && coupon.TimesUsed >= coupon.MaxUses.Value)
                throw new ValidationException("This coupon has reached its maximum number of uses.");

            if (coupon.MinOrderValue.HasValue && subTotal < coupon.MinOrderValue.Value)
                throw new ValidationException(
                    $"Order subtotal must be at least {coupon.MinOrderValue.Value:C} to use this coupon.");

            decimal discount = coupon.Type switch
            {
                CouponType.Percentage => subTotal * (coupon.Value / 100m),
                CouponType.FixedAmount => coupon.Value,
                CouponType.FreeShipping => 0m,
                _ => 0m
            };

            return (discount, coupon);
        }

        public async Task RecordCouponUsageAsync(string couponCode, Guid userId, Guid orderId)
        {
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == couponCode)
                ?? throw new NotFoundException($"Coupon '{couponCode}' not found.");

            var usage = new CouponUsage
            {
                Id = Guid.NewGuid(),
                CouponId = coupon.Id,
                UserId = userId,
                OrderId = orderId,
                UsedAt = DateTime.UtcNow
            };

            _context.CouponUsages.Add(usage);
            coupon.TimesUsed++;

            await _context.SaveChangesAsync();
        }

        public async Task<decimal?> GetFlashSalePriceAsync(Guid productId)
        {
            var prices = await GetFlashSalePricesAsync(new[] { productId });
            return prices.GetValueOrDefault(productId);
        }

        public async Task<Dictionary<Guid, decimal>> GetFlashSalePricesAsync(IEnumerable<Guid> productIds)
        {
            var now = DateTime.UtcNow;
            var ids = productIds.Distinct().ToList();

            return await _context.FlashSales
                .Where(fs => ids.Contains(fs.ProductId) && fs.IsActive && fs.StartsAt <= now && fs.EndsAt >= now)
                .ToDictionaryAsync(fs => fs.ProductId, fs => fs.SalePrice);
        }

        public async Task AwardLoyaltyPointsAsync(Guid userId, decimal orderTotal, Guid orderId)
        {
            int points = (int)orderTotal;

            var transaction = new LoyaltyTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = LoyaltyTransactionType.Earned,
                Points = points,
                Description = $"Points earned for order {orderId}",
                OrderId = orderId,
                CreatedAt = DateTime.UtcNow
            };

            _context.LoyaltyTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<LoyaltyBalanceResponse> GetLoyaltyBalanceAsync(Guid userId)
        {
            var balance = await _context.LoyaltyTransactions
                .Where(lt => lt.UserId == userId)
                .SumAsync(lt => lt.Points);

            return new LoyaltyBalanceResponse(balance, balance / 100.0m);
        }

        public async Task<decimal> RedeemLoyaltyPointsAsync(Guid userId, int points, Guid orderId)
        {
            if (points <= 0)
                throw new ValidationException("Points to redeem must be greater than zero.");

            var currentBalance = await GetLoyaltyBalanceAsync(userId);

            if (currentBalance.Points < points)
                throw new ValidationException(
                    $"Insufficient loyalty points. Available: {currentBalance.Points}, requested: {points}.");

            var transaction = new LoyaltyTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = LoyaltyTransactionType.Redeemed,
                Points = -points,
                Description = $"Points redeemed for order {orderId}",
                OrderId = orderId,
                CreatedAt = DateTime.UtcNow
            };

            _context.LoyaltyTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return points / 100.0m;
        }
    }
}
