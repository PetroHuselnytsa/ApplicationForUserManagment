using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Common;
using TestFirstProject.DTOs.Orders;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly PersonsContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly IPromotionService _promotionService;

    public OrderService(
        PersonsContext context,
        IInventoryService inventoryService,
        IPromotionService promotionService)
    {
        _context = context;
        _inventoryService = inventoryService;
        _promotionService = promotionService;
    }

    public async Task<PaginatedResult<OrderListDto>> GetUserOrdersAsync(Guid userId, int page, int pageSize)
    {
        var query = _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync();

        var orders = await query
            .Include(o => o.Items)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderListDto(
                o.Id, o.Status, o.TotalAmount,
                o.Items.Count, o.ShippingMethod, o.CreatedAt
            ))
            .ToListAsync();

        return new PaginatedResult<OrderListDto>(
            orders, totalCount, page, pageSize,
            (int)Math.Ceiling(totalCount / (double)pageSize)
        );
    }

    public async Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId, Guid userId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.Coupon)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null) return null;

        return MapToDetailDto(order);
    }

    public async Task<OrderDetailDto> CancelOrderAsync(Guid orderId, Guid userId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.Coupon)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new NotFoundException($"Order '{orderId}' not found.");

        // Validate state transition
        OrderStateMachine.ValidateTransition(order.Status, OrderStatus.Cancelled);

        var previousStatus = order.Status;
        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        // Record state change
        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = previousStatus,
            ToStatus = OrderStatus.Cancelled,
            ChangedByUserId = userId,
            Notes = "Order cancelled by customer",
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Release reserved stock
        foreach (var item in order.Items)
        {
            await _inventoryService.ReleaseStockAsync(item.ProductVariantId, item.Quantity, order.Id, userId);
        }

        return MapToDetailDto(order);
    }

    public async Task<OrderTrackingDto> GetTrackingInfoAsync(Guid orderId, Guid userId)
    {
        var order = await _context.Orders
            .Include(o => o.Shipment)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new NotFoundException($"Order '{orderId}' not found.");

        ShipmentDto? shipmentDto = null;
        if (order.Shipment != null)
        {
            var s = order.Shipment;
            shipmentDto = new ShipmentDto(
                s.Id, s.Method.ToString(), s.TrackingNumber, s.Carrier,
                s.ShippingCost, s.ShippedAt, s.EstimatedDeliveryDate, s.DeliveredAt
            );
        }

        return new OrderTrackingDto(order.Id, order.Status, shipmentDto);
    }

    // --- Admin Operations ---

    public async Task<PaginatedResult<OrderListDto>> GetAllOrdersAsync(AdminOrderFilterRequest filter)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsQueryable();

        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);

        if (filter.UserId.HasValue)
            query = query.Where(o => o.UserId == filter.UserId.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);

        var totalCount = await query.CountAsync();

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(o => new OrderListDto(
                o.Id, o.Status, o.TotalAmount,
                o.Items.Count, o.ShippingMethod, o.CreatedAt
            ))
            .ToListAsync();

        return new PaginatedResult<OrderListDto>(
            orders, totalCount, filter.Page, filter.PageSize,
            (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        );
    }

    public async Task<OrderDetailDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, Guid actorUserId, string? notes)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.Coupon)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException($"Order '{orderId}' not found.");

        // Validate transition through state machine
        OrderStateMachine.ValidateTransition(order.Status, newStatus);

        var previousStatus = order.Status;
        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = previousStatus,
            ToStatus = newStatus,
            ChangedByUserId = actorUserId,
            Notes = notes,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Side effects based on new status
        switch (newStatus)
        {
            case OrderStatus.Cancelled:
                // Release reserved stock
                foreach (var item in order.Items)
                {
                    await _inventoryService.ReleaseStockAsync(item.ProductVariantId, item.Quantity, order.Id, actorUserId);
                }
                break;

            case OrderStatus.Delivered:
                // Confirm sale (move from reserved to sold) and award loyalty points
                foreach (var item in order.Items)
                {
                    await _inventoryService.ConfirmSaleAsync(item.ProductVariantId, item.Quantity, order.Id, actorUserId);
                }
                await _promotionService.AwardLoyaltyPointsAsync(order.UserId, order.TotalAmount, order.Id);
                break;
        }

        return MapToDetailDto(order);
    }

    public async Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to)
    {
        var deliveredOrders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Delivered
                     && o.CreatedAt >= from
                     && o.CreatedAt <= to)
            .ToListAsync();

        var totalOrders = deliveredOrders.Count;
        var totalRevenue = deliveredOrders.Sum(o => o.TotalAmount);
        var totalDiscounts = deliveredOrders.Sum(o => o.DiscountAmount + o.LoyaltyDiscount);
        var itemsSold = deliveredOrders.Sum(o => o.Items.Sum(i => i.Quantity));
        var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        return new SalesReportDto(from, to, totalOrders, totalRevenue, totalDiscounts, itemsSold, avgOrderValue);
    }

    // --- Private helpers ---

    private static OrderDetailDto MapToDetailDto(Order order)
    {
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
