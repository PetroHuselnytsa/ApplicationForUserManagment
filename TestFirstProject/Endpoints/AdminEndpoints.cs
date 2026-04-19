using TestFirstProject.DTOs.Admin;
using TestFirstProject.DTOs.Orders;
using TestFirstProject.Extensions;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin")
            .RequireAuthorization();

        // GET /api/admin/orders — all orders with filters
        group.MapGet("/orders", async (
            IOrderService orderService,
            string? status,
            Guid? userId,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 20) =>
        {
            OrderStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var parsed))
                statusEnum = parsed;

            var filter = new AdminOrderFilterRequest(statusEnum, userId, fromDate, toDate, page, pageSize);
            var result = await orderService.GetAllOrdersAsync(filter);
            return Results.Ok(result);
        });

        // PUT /api/admin/orders/{id}/status — advance order state
        group.MapPut("/orders/{id:guid}/status", async (
            Guid id,
            UpdateOrderStatusRequest request,
            IOrderService orderService,
            HttpContext context) =>
        {
            var actorId = context.GetUserId();

            if (!Enum.TryParse<OrderStatus>(request.NewStatus, true, out var newStatus))
                return Results.BadRequest(new { error = $"Invalid status: '{request.NewStatus}'" });

            var order = await orderService.UpdateOrderStatusAsync(id, newStatus, actorId, request.Notes);
            return Results.Ok(order);
        });

        // GET /api/admin/inventory — stock levels
        group.MapGet("/inventory", async (
            IInventoryService inventoryService,
            Guid? productVariantId,
            Guid? warehouseId) =>
        {
            var levels = await inventoryService.GetStockLevelsAsync(productVariantId, warehouseId);
            return Results.Ok(levels);
        });

        // GET /api/admin/inventory/low-stock — items below threshold
        group.MapGet("/inventory/low-stock", async (IInventoryService inventoryService) =>
        {
            var items = await inventoryService.GetLowStockItemsAsync();
            return Results.Ok(items);
        });

        // POST /api/admin/inventory/restock — create restock request
        group.MapPost("/inventory/restock", async (
            CreateRestockRequest request,
            IInventoryService inventoryService,
            HttpContext context) =>
        {
            var userId = context.GetUserId();
            var restockReq = await inventoryService.CreateRestockRequestAsync(request, userId);
            return Results.Created($"/api/admin/inventory/restock/{restockReq.Id}", restockReq);
        });

        // GET /api/admin/inventory/restock — list restock requests
        group.MapGet("/inventory/restock", async (IInventoryService inventoryService) =>
        {
            var requests = await inventoryService.GetRestockRequestsAsync();
            return Results.Ok(requests);
        });

        // GET /api/admin/reports/sales — daily/monthly sales summary
        group.MapGet("/reports/sales", async (
            IOrderService orderService,
            DateTime from,
            DateTime to) =>
        {
            var report = await orderService.GetSalesReportAsync(from, to);
            return Results.Ok(report);
        });
    }
}
