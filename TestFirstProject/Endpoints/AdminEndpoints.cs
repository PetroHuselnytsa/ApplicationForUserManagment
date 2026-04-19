using TestFirstProject.DTOs;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    public static class AdminEndpoints
    {
        public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/admin")
                           .WithTags("Admin")
                           .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"));

            group.MapGet("/orders", async (IOrderService orderService, int? page, int? pageSize, string? status) =>
            {
                var result = await orderService.GetAllOrdersAsync(page ?? 1, pageSize ?? 20, status);
                return Results.Ok(result);
            })
            .WithName("GetAllOrders");

            group.MapPut("/orders/{id:guid}/status", async (Guid id, UpdateOrderStatusRequest request, IOrderService orderService, HttpContext httpContext) =>
            {
                var adminUserId = httpContext.GetUserId();
                var result = await orderService.UpdateOrderStatusAsync(id, request, adminUserId);
                return Results.Ok(result);
            })
            .WithName("UpdateOrderStatus");

            group.MapGet("/inventory", async (IInventoryService inventoryService) =>
            {
                var result = await inventoryService.GetStockLevelsAsync();
                return Results.Ok(result);
            })
            .WithName("GetInventory");

            group.MapGet("/inventory/low-stock", async (IInventoryService inventoryService) =>
            {
                var result = await inventoryService.GetLowStockItemsAsync();
                return Results.Ok(result);
            })
            .WithName("GetLowStockItems");

            group.MapPost("/inventory/restock", async (CreateRestockRequest request, IInventoryService inventoryService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await inventoryService.CreateRestockRequestAsync(request, userId);
                return Results.Created($"/api/admin/inventory/restock/{result.Id}", result);
            })
            .WithName("CreateRestockRequest");

            group.MapGet("/inventory/restock", async (IInventoryService inventoryService) =>
            {
                var result = await inventoryService.GetRestockRequestsAsync();
                return Results.Ok(result);
            })
            .WithName("GetRestockRequests");

            group.MapGet("/reports/sales", async (IOrderService orderService, string? period, DateTime? startDate, DateTime? endDate) =>
            {
                var result = await orderService.GetSalesReportAsync(period, startDate, endDate);
                return Results.Ok(result);
            })
            .WithName("GetSalesReport");
        }
    }
}
