using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations;

public class PromotionService : IPromotionService
{
    private readonly PersonsContext _context;

    // Loyalty: 1 point per $1 spent, 100 points = $1 discount
    private const int PointsPerDollar = 1;
    private const int PointsPerDollarRedemption = 100;

    public PromotionService(PersonsContext context)
    {
        _context = context;
    }

    public async Task<Coupon?> ValidateCouponAsync(string code, decimal orderTotal, Guid userId)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);

        if (coupon == null) return null;

        // Check expiry
        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt.Value < DateTime.UtcNow)
            throw new ValidationException("This coupon has expired.");

        // Check usage limit
        if (coupon.MaxUses.HasValue && coupon.CurrentUses >= coupon.MaxUses.Value)
            throw new ValidationException("This coupon has reached its maximum usage limit.");

        // Check minimum order value
        if (coupon.MinOrderValue.HasValue && orderTotal < coupon.MinOrderValue.Value)
            throw new ValidationException($"Minimum order value for this coupon is ${coupon.MinOrderValue.Value:F2}.");

        // Check if this user already used this coupon
        var alreadyUsed = await _context.CouponUsages
            .AnyAsync(u => u.CouponId == coupon.Id && u.UserId == userId);

        if (alreadyUsed)
            throw new ValidationException("You have already used this coupon.");

        return coupon;
    }

    public decimal CalculateCouponDiscount(Coupon coupon, decimal subtotal)
    {
        return coupon.Type switch
        {
            CouponType.Percentage => Math.Round(subtotal * coupon.DiscountValue / 100, 2),
            CouponType.Fixed => Math.Min(coupon.DiscountValue, subtotal), // Can't exceed subtotal
            CouponType.FreeShipping => 0, // Handled separately in shipping cost calculation
            _ => 0
        };
    }

    public async Task RecordCouponUsageAsync(Guid couponId, Guid userId, Guid orderId)
    {
        var coupon = await _context.Coupons.FindAsync(couponId)
            ?? throw new NotFoundException($"Coupon '{couponId}' not found.");

        coupon.CurrentUses++;

        _context.CouponUsages.Add(new CouponUsage
        {
            CouponId = couponId,
            UserId = userId,
            OrderId = orderId,
            UsedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task<decimal?> GetFlashSalePriceAsync(Guid productId)
    {
        var now = DateTime.UtcNow;

        var flashSale = await _context.FlashSales
            .Where(f => f.ProductId == productId
                     && f.IsActive
                     && f.StartTime <= now
                     && f.EndTime >= now)
            .OrderBy(f => f.SalePrice) // Use the lowest sale price if multiple active
            .FirstOrDefaultAsync();

        return flashSale?.SalePrice;
    }

    public async Task<int> GetLoyaltyPointsAsync(Guid userId)
    {
        return await _context.LoyaltyTransactions
            .Where(l => l.UserId == userId)
            .SumAsync(l => l.Points);
    }

    public async Task AwardLoyaltyPointsAsync(Guid userId, decimal orderTotal, Guid orderId)
    {
        var points = (int)Math.Floor(orderTotal) * PointsPerDollar;
        if (points <= 0) return;

        _context.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            UserId = userId,
            Points = points,
            Description = $"Points earned from order",
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task<decimal> RedeemLoyaltyPointsAsync(Guid userId, int points, Guid orderId)
    {
        if (points <= 0) return 0;

        var balance = await GetLoyaltyPointsAsync(userId);
        if (balance < points)
            throw new ValidationException($"Insufficient loyalty points. Balance: {balance}, Requested: {points}");

        var discountAmount = Math.Round((decimal)points / PointsPerDollarRedemption, 2);

        _context.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            UserId = userId,
            Points = -points, // Negative for redemption
            Description = $"Points redeemed for order discount",
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return discountAmount;
    }
}
