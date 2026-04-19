using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly PersonsContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IPromotionService _promotionService;
        private readonly IShippingService _shippingService;

        public CartService(
            PersonsContext context,
            IInventoryService inventoryService,
            IPromotionService promotionService,
            IShippingService shippingService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _promotionService = promotionService;
            _shippingService = shippingService;
        }

        public async Task<CartResponse> GetCartAsync(Guid userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return await MapCartToResponseAsync(cart);
        }

        public async Task<CartResponse> AddItemAsync(Guid userId, AddCartItemRequest request)
        {
            if (request.Quantity <= 0)
                throw new ValidationException("Quantity must be greater than zero.");

            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId && v.IsActive);

            if (variant == null || !variant.Product.IsActive)
                throw new NotFoundException("Product variant not found or inactive.");

            var available = await _inventoryService.GetAvailableStockAsync(request.ProductVariantId);

            var cart = await GetOrCreateCartAsync(userId);
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == request.ProductVariantId);

            var requiredQty = (existingItem?.Quantity ?? 0) + request.Quantity;
            ThrowIfInsufficientStock(available, requiredQty);

            if (existingItem != null)
            {
                existingItem.Quantity = requiredQty;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductVariantId = request.ProductVariantId,
                    Quantity = request.Quantity,
                    AddedAt = DateTime.UtcNow
                });
            }

            cart.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await MapCartToResponseAsync(cart);
        }

        public async Task<CartResponse> UpdateItemAsync(Guid userId, Guid itemId, UpdateCartItemRequest request)
        {
            if (request.Quantity <= 0)
                throw new ValidationException("Quantity must be greater than zero.");

            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId)
                ?? throw new NotFoundException("Cart item not found.");

            var available = await _inventoryService.GetAvailableStockAsync(item.ProductVariantId);
            ThrowIfInsufficientStock(available, request.Quantity);

            item.Quantity = request.Quantity;
            cart.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await MapCartToResponseAsync(cart);
        }

        public async Task<CartResponse> RemoveItemAsync(Guid userId, Guid itemId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId)
                ?? throw new NotFoundException("Cart item not found.");

            _context.CartItems.Remove(item);
            cart.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await MapCartToResponseAsync(cart);
        }

        public async Task<CartResponse> ApplyCouponAsync(Guid userId, ApplyCouponRequest request)
        {
            var cart = await GetOrCreateCartAsync(userId);

            // Validate coupon before saving — compute subtotal in the same pass
            var subTotal = await CalculateSubTotalAsync(cart);
            var (_, coupon) = await _promotionService.CalculateCouponDiscountAsync(request.CouponCode, subTotal);

            if (coupon == null)
                throw new NotFoundException("Invalid coupon code.");

            cart.CouponCode = request.CouponCode;
            cart.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await MapCartToResponseAsync(cart);
        }

        public async Task<OrderResponse> CheckoutAsync(Guid userId, CheckoutRequest request)
        {
            var cart = await GetOrCreateCartAsync(userId);

            if (!cart.Items.Any())
                throw new ValidationException("Cart is empty.");

            if (!Enum.TryParse<ShippingMethod>(request.ShippingMethod, true, out var shippingMethod))
                throw new ValidationException($"Invalid shipping method: {request.ShippingMethod}");

            var variantIds = cart.Items.Select(i => i.ProductVariantId).ToList();
            var variants = await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => variantIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id);

            var productIds = variants.Values.Select(v => v.ProductId).Distinct();
            var flashPrices = await _promotionService.GetFlashSalePricesAsync(productIds);

            decimal subTotal = 0;
            decimal totalWeight = 0;
            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cart.Items)
            {
                if (!variants.TryGetValue(cartItem.ProductVariantId, out var variant))
                    throw new AppException($"Product variant {cartItem.ProductVariantId} no longer exists.", 400);

                var unitPrice = flashPrices.GetValueOrDefault(variant.ProductId, variant.Product.BasePrice + variant.PriceDelta);
                var lineTotal = unitPrice * cartItem.Quantity;

                subTotal += lineTotal;
                totalWeight += variant.Product.Weight * cartItem.Quantity;

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductVariantId = variant.Id,
                    ProductName = variant.Product.Name,
                    VariantSku = variant.Sku,
                    UnitPrice = unitPrice,
                    Quantity = cartItem.Quantity,
                    LineTotal = lineTotal
                });
            }

            var (discountAmount, coupon) = await _promotionService.CalculateCouponDiscountAsync(cart.CouponCode, subTotal);

            var shippingCost = await _shippingService.CalculateShippingCostAsync(totalWeight, shippingMethod, request.ShippingZone);

            if (coupon?.Type == CouponType.FreeShipping)
                shippingCost = 0;

            var totalAmount = Math.Max(0, subTotal - discountAmount + shippingCost);
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

            var reserveItems = cart.Items.Select(i => (i.ProductVariantId, i.Quantity)).ToList();
            await _inventoryService.ReserveStockAsync(reserveItems, orderNumber);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                UserId = userId,
                Status = OrderStatus.Pending,
                ShippingAddress = request.ShippingAddress,
                BillingAddress = request.BillingAddress,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                ShippingCost = shippingCost,
                TotalAmount = totalAmount,
                CouponCode = cart.CouponCode,
                ShippingMethod = shippingMethod,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = orderItems,
                StatusHistory = new List<OrderStatusHistory>
                {
                    new OrderStatusHistory
                    {
                        Id = Guid.NewGuid(),
                        FromStatus = OrderStatus.Draft,
                        ToStatus = OrderStatus.Pending,
                        ChangedByUserId = userId,
                        Notes = "Order created from cart checkout.",
                        ChangedAt = DateTime.UtcNow
                    }
                }
            };

            _context.Orders.Add(order);

            if (!string.IsNullOrEmpty(cart.CouponCode) && coupon != null)
            {
                await _promotionService.RecordCouponUsageAsync(cart.CouponCode, userId, order.Id);
            }

            await _shippingService.CreateShipmentAsync(order.Id, shippingMethod, totalWeight, request.ShippingZone);

            _context.CartItems.RemoveRange(cart.Items);
            cart.CouponCode = null;
            cart.LastActivityAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _promotionService.AwardLoyaltyPointsAsync(userId, totalAmount, order.Id);

            return OrderService.MapToResponse(order);
        }

        // --- Private Helpers ---

        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
                return cart;

            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        private async Task<decimal> CalculateSubTotalAsync(Cart cart)
        {
            var productIds = cart.Items.Select(i => i.ProductVariant.ProductId).Distinct();
            var flashPrices = await _promotionService.GetFlashSalePricesAsync(productIds);

            return cart.Items.Sum(item =>
            {
                var variant = item.ProductVariant;
                var unitPrice = flashPrices.GetValueOrDefault(variant.ProductId, variant.Product.BasePrice + variant.PriceDelta);
                return unitPrice * item.Quantity;
            });
        }

        private async Task<CartResponse> MapCartToResponseAsync(Cart cart)
        {
            var productIds = cart.Items.Select(i => i.ProductVariant.ProductId).Distinct();
            var flashPrices = await _promotionService.GetFlashSalePricesAsync(productIds);

            var items = new List<CartItemResponse>();
            decimal subTotal = 0;

            foreach (var item in cart.Items)
            {
                var variant = item.ProductVariant;
                var product = variant.Product;
                var unitPrice = flashPrices.GetValueOrDefault(variant.ProductId, product.BasePrice + variant.PriceDelta);
                var lineTotal = unitPrice * item.Quantity;
                subTotal += lineTotal;

                items.Add(new CartItemResponse(
                    item.Id,
                    item.ProductVariantId,
                    product.Name,
                    variant.Sku,
                    variant.Size,
                    variant.Color,
                    unitPrice,
                    item.Quantity,
                    lineTotal));
            }

            var (discountAmount, _) = await _promotionService.CalculateCouponDiscountAsync(cart.CouponCode, subTotal);
            var total = Math.Max(0, subTotal - discountAmount);

            return new CartResponse(
                cart.Id,
                items,
                cart.CouponCode,
                subTotal,
                discountAmount,
                total,
                cart.LastActivityAt);
        }

        private static void ThrowIfInsufficientStock(int available, int requested)
        {
            if (available < requested)
                throw new AppException($"Insufficient stock. Only {available} available.", 400);
        }
    }
}
