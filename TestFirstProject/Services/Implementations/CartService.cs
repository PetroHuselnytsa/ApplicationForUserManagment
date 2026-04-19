using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Cart;
using TestFirstProject.DTOs.Orders;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations;

public class CartService : ICartService
{
    private readonly PersonsContext _context;
    private readonly IPromotionService _promotionService;
    private readonly IInventoryService _inventoryService;
    private readonly IShippingService _shippingService;

    public CartService(
        PersonsContext context,
        IPromotionService promotionService,
        IInventoryService inventoryService,
        IShippingService shippingService)
    {
        _context = context;
        _promotionService = promotionService;
        _inventoryService = inventoryService;
        _shippingService = shippingService;
    }

    public async Task<CartDto> GetCartAsync(Guid userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return await MapToCartDto(cart);
    }

    public async Task<CartDto> AddItemAsync(Guid userId, AddCartItemRequest request)
    {
        if (request.Quantity <= 0)
            throw new ValidationException("Quantity must be greater than zero.");

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId && v.IsActive)
            ?? throw new NotFoundException($"Product variant '{request.ProductVariantId}' not found or inactive.");

        // Check stock availability
        var available = await _inventoryService.GetAvailableStockAsync(request.ProductVariantId);
        if (available < request.Quantity)
            throw new ValidationException($"Insufficient stock. Available: {available}");

        var cart = await GetOrCreateCartAsync(userId);

        // Check if item already in cart — update quantity instead
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == request.ProductVariantId);
        if (existingItem != null)
        {
            var newQty = existingItem.Quantity + request.Quantity;
            if (available < newQty)
                throw new ValidationException($"Insufficient stock for total quantity. Available: {available}");

            existingItem.Quantity = newQty;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                CartId = cart.Id,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                AddedAt = DateTime.UtcNow
            });
        }

        cart.LastActivityAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await MapToCartDto(cart);
    }

    public async Task<CartDto> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request)
    {
        if (request.Quantity <= 0)
            throw new ValidationException("Quantity must be greater than zero.");

        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId)
            ?? throw new NotFoundException($"Cart item '{cartItemId}' not found.");

        // Check stock
        var available = await _inventoryService.GetAvailableStockAsync(item.ProductVariantId);
        if (available < request.Quantity)
            throw new ValidationException($"Insufficient stock. Available: {available}");

        item.Quantity = request.Quantity;
        cart.LastActivityAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await MapToCartDto(cart);
    }

    public async Task<CartDto> RemoveItemAsync(Guid userId, Guid cartItemId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId)
            ?? throw new NotFoundException($"Cart item '{cartItemId}' not found.");

        _context.CartItems.Remove(item);
        cart.LastActivityAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await MapToCartDto(cart);
    }

    public async Task<CartDto> ApplyCouponAsync(Guid userId, ApplyCouponRequest request)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var subtotal = await CalculateSubtotal(cart);

        var coupon = await _promotionService.ValidateCouponAsync(request.CouponCode, subtotal, userId)
            ?? throw new NotFoundException($"Coupon code '{request.CouponCode}' not found.");

        cart.AppliedCouponId = coupon.Id;
        cart.LastActivityAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await MapToCartDto(cart);
    }

    public async Task<OrderDetailDto> CheckoutAsync(Guid userId, CheckoutRequest request)
    {
        var cart = await GetOrCreateCartAsync(userId);

        if (!cart.Items.Any())
            throw new ValidationException("Cart is empty. Add items before checkout.");

        // Parse shipping method
        if (!Enum.TryParse<ShippingMethodType>(request.ShippingMethod, true, out var shippingMethod))
            throw new ValidationException($"Invalid shipping method: '{request.ShippingMethod}'. Valid values: Standard, Express, SameDay, Pickup");

        var now = DateTime.UtcNow;

        // Calculate subtotal
        var subtotal = await CalculateSubtotal(cart);

        // Calculate discount from coupon
        decimal discountAmount = 0;
        Coupon? coupon = null;
        if (cart.AppliedCouponId.HasValue)
        {
            coupon = await _context.Coupons.FindAsync(cart.AppliedCouponId.Value);
            if (coupon != null)
            {
                discountAmount = _promotionService.CalculateCouponDiscount(coupon, subtotal);
            }
        }

        // Calculate loyalty discount
        decimal loyaltyDiscount = 0;
        if (request.LoyaltyPointsToRedeem > 0)
        {
            // Validate loyalty points — actual deduction happens after order creation
            var balance = await _promotionService.GetLoyaltyPointsAsync(userId);
            if (balance < request.LoyaltyPointsToRedeem)
                throw new ValidationException($"Insufficient loyalty points. Balance: {balance}");
            loyaltyDiscount = Math.Round((decimal)request.LoyaltyPointsToRedeem / 100, 2);
        }

        // Calculate total weight for shipping
        var totalWeight = 0m;
        foreach (var item in cart.Items)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstAsync(v => v.Id == item.ProductVariantId);
            totalWeight += variant.Product.Weight * item.Quantity;
        }

        // Calculate shipping cost
        var shippingCost = _shippingService.CalculateShippingCost(shippingMethod, totalWeight);

        // Free shipping coupon
        if (coupon?.Type == CouponType.FreeShipping)
            shippingCost = 0;

        var totalAmount = subtotal - discountAmount - loyaltyDiscount + shippingCost;
        if (totalAmount < 0) totalAmount = 0;

        // Create order
        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Draft,
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress,
            SubTotal = subtotal,
            DiscountAmount = discountAmount,
            ShippingCost = shippingCost,
            TotalAmount = totalAmount,
            ShippingMethod = shippingMethod,
            CouponId = cart.AppliedCouponId,
            LoyaltyPointsUsed = request.LoyaltyPointsToRedeem,
            LoyaltyDiscount = loyaltyDiscount,
            Notes = request.Notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Orders.Add(order);

        // Create order items with price snapshots
        foreach (var cartItem in cart.Items)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstAsync(v => v.Id == cartItem.ProductVariantId);

            // Check for flash sale price
            var salePrice = await _promotionService.GetFlashSalePriceAsync(variant.ProductId);
            var unitPrice = salePrice ?? (variant.Product.BasePrice + variant.PriceDelta);

            var variantDesc = string.Join(", ",
                new[] { variant.Size, variant.Color, variant.Material }
                .Where(s => !string.IsNullOrEmpty(s)));

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductVariantId = cartItem.ProductVariantId,
                ProductName = variant.Product.Name,
                VariantDescription = variantDesc,
                Quantity = cartItem.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = unitPrice * cartItem.Quantity
            };

            _context.OrderItems.Add(orderItem);
        }

        // Add initial status history
        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = OrderStatus.Draft,
            ToStatus = OrderStatus.Draft,
            ChangedByUserId = userId,
            Notes = "Order created from cart checkout",
            ChangedAt = now
        });

        await _context.SaveChangesAsync();

        // Reserve stock for each item
        foreach (var cartItem in cart.Items)
        {
            await _inventoryService.ReserveStockAsync(cartItem.ProductVariantId, cartItem.Quantity, order.Id, userId);
        }

        // Record coupon usage
        if (cart.AppliedCouponId.HasValue)
        {
            await _promotionService.RecordCouponUsageAsync(cart.AppliedCouponId.Value, userId, order.Id);
        }

        // Redeem loyalty points
        if (request.LoyaltyPointsToRedeem > 0)
        {
            await _promotionService.RedeemLoyaltyPointsAsync(userId, request.LoyaltyPointsToRedeem, order.Id);
        }

        // Transition to Pending
        OrderStateMachine.ValidateTransition(order.Status, OrderStatus.Pending);
        order.Status = OrderStatus.Pending;
        order.UpdatedAt = DateTime.UtcNow;

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = OrderStatus.Draft,
            ToStatus = OrderStatus.Pending,
            ChangedByUserId = userId,
            Notes = "Order submitted for payment",
            ChangedAt = DateTime.UtcNow
        });

        // Create shipment record
        await _shippingService.CreateShipmentAsync(order.Id, shippingMethod, totalWeight, "domestic");

        // Clear the cart
        _context.CartItems.RemoveRange(cart.Items);
        cart.AppliedCouponId = null;
        cart.LastActivityAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Return order details
        return await GetOrderDetailDto(order.Id);
    }

    // --- Private helpers ---

    private async Task<Cart> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .Include(c => c.AppliedCoupon)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    private async Task<decimal> CalculateSubtotal(Cart cart)
    {
        decimal subtotal = 0;
        foreach (var item in cart.Items)
        {
            var variant = item.ProductVariant ?? await _context.ProductVariants
                .Include(v => v.Product)
                .FirstAsync(v => v.Id == item.ProductVariantId);

            var salePrice = await _promotionService.GetFlashSalePriceAsync(variant.ProductId);
            var unitPrice = salePrice ?? (variant.Product.BasePrice + variant.PriceDelta);
            subtotal += unitPrice * item.Quantity;
        }
        return subtotal;
    }

    private async Task<CartDto> MapToCartDto(Cart cart)
    {
        var items = new List<CartItemDto>();
        decimal subtotal = 0;

        foreach (var item in cart.Items)
        {
            var variant = item.ProductVariant ?? await _context.ProductVariants
                .Include(v => v.Product)
                .FirstAsync(v => v.Id == item.ProductVariantId);

            var salePrice = await _promotionService.GetFlashSalePriceAsync(variant.ProductId);
            var unitPrice = salePrice ?? (variant.Product.BasePrice + variant.PriceDelta);
            var totalPrice = unitPrice * item.Quantity;
            subtotal += totalPrice;

            var variantDesc = string.Join(", ",
                new[] { variant.Size, variant.Color, variant.Material }
                .Where(s => !string.IsNullOrEmpty(s)));

            items.Add(new CartItemDto(
                item.Id, item.ProductVariantId,
                variant.Product.Name, variantDesc,
                unitPrice, item.Quantity, totalPrice,
                variant.Product.ImageUrls
            ));
        }

        decimal discountAmount = 0;
        if (cart.AppliedCoupon != null)
        {
            discountAmount = _promotionService.CalculateCouponDiscount(cart.AppliedCoupon, subtotal);
        }

        return new CartDto(
            cart.Id, items,
            cart.AppliedCoupon?.Code,
            subtotal, discountAmount,
            subtotal - discountAmount,
            cart.LastActivityAt
        );
    }

    private async Task<OrderDetailDto> GetOrderDetailDto(Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.Coupon)
            .FirstAsync(o => o.Id == orderId);

        var items = order.Items.Select(i => new OrderItemDto(
            i.Id, i.ProductVariantId, i.ProductName, i.VariantDescription,
            i.Quantity, i.UnitPrice, i.TotalPrice
        )).ToList();

        var history = order.StatusHistory
            .OrderBy(h => h.ChangedAt)
            .Select(h => new OrderStatusHistoryDto(
                h.FromStatus, h.ToStatus, h.ChangedByUserId, h.Notes, h.ChangedAt
            )).ToList();

        return new OrderDetailDto(
            order.Id, order.Status,
            order.ShippingAddress, order.BillingAddress,
            order.SubTotal, order.DiscountAmount, order.ShippingCost, order.TotalAmount,
            order.ShippingMethod, order.LoyaltyPointsUsed, order.LoyaltyDiscount,
            order.Coupon?.Code, order.Notes, order.CreatedAt,
            items, history
        );
    }
}
