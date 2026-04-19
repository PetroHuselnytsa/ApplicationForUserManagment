using TestFirstProject.DTOs;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    public static class OrderEndpoints
    {
        public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/orders")
                           .WithTags("Orders")
                           .RequireAuthorization();

            group.MapGet("/", async (IOrderService orderService, HttpContext httpContext, int? page, int? pageSize) =>
            {
                var userId = httpContext.GetUserId();
                var result = await orderService.GetUserOrdersAsync(userId, page ?? 1, pageSize ?? 20);
                return Results.Ok(result);
            })
            .WithName("GetUserOrders");

            group.MapGet("/{id:guid}", async (Guid id, IOrderService orderService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await orderService.GetOrderByIdAsync(id, userId);
                return Results.Ok(result);
            })
            .WithName("GetOrderById");

            group.MapPost("/{id:guid}/cancel", async (Guid id, IOrderService orderService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await orderService.CancelOrderAsync(id, userId);
                return Results.Ok(result);
            })
            .WithName("CancelOrder");

            group.MapGet("/{id:guid}/tracking", async (Guid id, IOrderService orderService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await orderService.GetTrackingAsync(id, userId);
                return Results.Ok(result);
            })
            .WithName("GetOrderTracking");
        }
    }
}
