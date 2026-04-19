using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders")
            .RequireAuthorization();

        // GET /api/orders — list user's orders
        group.MapGet("/", async (IOrderService orderService, HttpContext context, int page = 1, int pageSize = 20) =>
        {
            var userId = context.GetUserId();
            var result = await orderService.GetUserOrdersAsync(userId, page, pageSize);
            return Results.Ok(result);
        });

        // GET /api/orders/{id} — order detail
        group.MapGet("/{id:guid}", async (Guid id, IOrderService orderService, HttpContext context) =>
        {
            var userId = context.GetUserId();
            var order = await orderService.GetOrderByIdAsync(id, userId);
            return order == null ? Results.NotFound() : Results.Ok(order);
        });

        // POST /api/orders/{id}/cancel — cancel order
        group.MapPost("/{id:guid}/cancel", async (Guid id, IOrderService orderService, HttpContext context) =>
        {
            var userId = context.GetUserId();
            var order = await orderService.CancelOrderAsync(id, userId);
            return Results.Ok(order);
        });

        // GET /api/orders/{id}/tracking — shipping tracking info
        group.MapGet("/{id:guid}/tracking", async (Guid id, IOrderService orderService, HttpContext context) =>
        {
            var userId = context.GetUserId();
            var tracking = await orderService.GetTrackingInfoAsync(id, userId);
            return Results.Ok(tracking);
        });
    }
}
