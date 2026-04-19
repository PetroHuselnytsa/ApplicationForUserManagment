using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly PersonsContext _context;
        private readonly IInventoryService _inventoryService;

        public OrderService(
            PersonsContext context,
            IInventoryService inventoryService,
            IShippingService shippingService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public async Task<PaginatedResponse<OrderListResponse>> GetUserOrdersAsync(Guid userId, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt);

            var totalCount = await query.CountAsync();
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(o => o.Items)
                .ToListAsync();

            return MapToPaginatedList(orders, totalCount, page, pageSize);
        }

        public async Task<OrderResponse> GetOrderByIdAsync(Guid orderId, Guid userId)
        {
            var order = await LoadOrderWithHistoryAsync(o => o.Id == orderId && o.UserId == userId)
                ?? throw new NotFoundException("Order not found.");

            return MapToResponse(order);
        }

        public async Task<OrderResponse> CancelOrderAsync(Guid orderId, Guid userId)
        {
            var order = await LoadOrderWithHistoryAsync(o => o.Id == orderId && o.UserId == userId)
                ?? throw new NotFoundException("Order not found.");

            OrderStateMachine.ValidateTransition(order.Status, OrderStatus.Cancelled);

            var previousStatus = order.Status;
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            order.StatusHistory.Add(new OrderStatusHistory
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                FromStatus = previousStatus,
                ToStatus = OrderStatus.Cancelled,
                ChangedByUserId = userId,
                Notes = "Cancelled by user.",
                ChangedAt = DateTime.UtcNow
            });

            var releaseItems = order.Items
                .Select(i => (i.ProductVariantId, i.Quantity))
                .ToList();
            await _inventoryService.ReleaseStockAsync(releaseItems, order.OrderNumber);

            await _context.SaveChangesAsync();
            return MapToResponse(order);
        }

        public async Task<TrackingResponse> GetTrackingAsync(Guid orderId, Guid userId)
        {
            var order = await _context.Orders
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
                ?? throw new NotFoundException("Order not found.");

            return new TrackingResponse(
                order.Id,
                order.OrderNumber,
                order.Status.ToString(),
                order.Shipment?.Status.ToString(),
                order.Shipment?.TrackingNumber,
                order.Shipment?.Carrier,
                order.Shipment?.EstimatedDeliveryDate,
                order.Shipment?.ShippedAt,
                order.Shipment?.DeliveredAt);
        }

        // --- Admin Methods ---

        public async Task<PaginatedResponse<OrderListResponse>> GetAllOrdersAsync(int page, int pageSize, string? status)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var statusFilter))
            {
                query = query.Where(o => o.Status == statusFilter);
            }

            query = query.OrderByDescending(o => o.CreatedAt);

            var totalCount = await query.CountAsync();
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(o => o.Items)
                .ToListAsync();

            return MapToPaginatedList(orders, totalCount, page, pageSize);
        }

        public async Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, Guid adminUserId)
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
                throw new ValidationException($"Invalid status: {request.Status}");

            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                    .ThenInclude(h => h.ChangedByUser)
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new NotFoundException("Order not found.");

            OrderStateMachine.ValidateTransition(order.Status, newStatus);

            var previousStatus = order.Status;
            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            order.StatusHistory.Add(new OrderStatusHistory
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                FromStatus = previousStatus,
                ToStatus = newStatus,
                ChangedByUserId = adminUserId,
                Notes = request.Notes,
                ChangedAt = DateTime.UtcNow
            });

            switch (newStatus)
            {
                case OrderStatus.Shipped:
                    if (order.Shipment != null)
                    {
                        order.Shipment.Status = ShipmentStatus.InTransit;
                        order.Shipment.ShippedAt = DateTime.UtcNow;
                    }
                    var soldItems = order.Items
                        .Select(i => (i.ProductVariantId, i.Quantity))
                        .ToList();
                    await _inventoryService.ConfirmStockSoldAsync(soldItems, order.OrderNumber);
                    break;

                case OrderStatus.Delivered:
                    if (order.Shipment != null)
                    {
                        order.Shipment.Status = ShipmentStatus.Delivered;
                        order.Shipment.DeliveredAt = DateTime.UtcNow;
                    }
                    break;

                case OrderStatus.Cancelled:
                    var releaseItems = order.Items
                        .Select(i => (i.ProductVariantId, i.Quantity))
                        .ToList();
                    await _inventoryService.ReleaseStockAsync(releaseItems, order.OrderNumber);
                    break;
            }

            await _context.SaveChangesAsync();
            return MapToResponse(order);
        }

        public async Task<SalesReportResponse> GetSalesReportAsync(string? period, DateTime? startDate, DateTime? endDate)
        {
            var now = DateTime.UtcNow;
            var periodEnd = endDate ?? now;
            var periodStart = startDate ?? period?.ToLower() switch
            {
                "daily" => now.Date,
                "weekly" => now.Date.AddDays(-7),
                "yearly" => now.Date.AddYears(-1),
                _ => now.Date.AddDays(-30)
            };

            var excludedStatuses = new[] { OrderStatus.Cancelled, OrderStatus.Draft };

            var baseQuery = _context.Orders
                .Where(o => o.CreatedAt >= periodStart && o.CreatedAt <= periodEnd)
                .Where(o => !excludedStatuses.Contains(o.Status));

            var totalRevenue = await baseQuery.SumAsync(o => o.TotalAmount);
            var totalOrders = await baseQuery.CountAsync();
            var totalItemsSold = await baseQuery.SelectMany(o => o.Items).SumAsync(i => i.Quantity);
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var topProducts = await baseQuery
                .SelectMany(o => o.Items)
                .GroupBy(i => i.ProductName)
                .Select(g => new TopProductResponse(
                    g.Key,
                    g.Sum(i => i.Quantity),
                    g.Sum(i => i.LineTotal)))
                .OrderByDescending(p => p.Revenue)
                .Take(10)
                .ToListAsync();

            return new SalesReportResponse(
                periodStart, periodEnd, totalRevenue, totalOrders,
                totalItemsSold, averageOrderValue, topProducts);
        }

        // --- Shared Helpers ---

        internal static OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse(
                order.Id,
                order.OrderNumber,
                order.Status.ToString(),
                order.ShippingAddress,
                order.BillingAddress,
                order.SubTotal,
                order.DiscountAmount,
                order.ShippingCost,
                order.TotalAmount,
                order.CouponCode,
                order.ShippingMethod.ToString(),
                order.CreatedAt,
                order.Items.Select(i => new OrderItemResponse(
                    i.Id, i.ProductName, i.VariantSku, i.UnitPrice, i.Quantity, i.LineTotal)).ToList(),
                order.StatusHistory
                    .OrderBy(h => h.ChangedAt)
                    .Select(h => new OrderStatusHistoryResponse(
                        h.FromStatus.ToString(),
                        h.ToStatus.ToString(),
                        h.ChangedByUser?.Username ?? "System",
                        h.Notes,
                        h.ChangedAt))
                    .ToList());
        }

        // --- Private Helpers ---

        private async Task<Order?> LoadOrderWithHistoryAsync(System.Linq.Expressions.Expression<Func<Order, bool>> predicate)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                    .ThenInclude(h => h.ChangedByUser)
                .FirstOrDefaultAsync(predicate);
        }

        private static PaginatedResponse<OrderListResponse> MapToPaginatedList(
            List<Order> orders, int totalCount, int page, int pageSize)
        {
            var items = orders.Select(o => new OrderListResponse(
                o.Id, o.OrderNumber, o.Status.ToString(), o.TotalAmount,
                o.Items.Count, o.CreatedAt)).ToList();

            return new PaginatedResponse<OrderListResponse>(
                items, totalCount, page, pageSize,
                (int)Math.Ceiling(totalCount / (double)pageSize));
        }
    }
}
