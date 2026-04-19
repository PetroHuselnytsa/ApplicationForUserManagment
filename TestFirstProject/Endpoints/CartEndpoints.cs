using TestFirstProject.DTOs;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    public static class CartEndpoints
    {
        public static void MapCartEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/cart")
                           .WithTags("Cart")
                           .RequireAuthorization();

            group.MapGet("/", async (ICartService cartService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await cartService.GetCartAsync(userId);
                return Results.Ok(result);
            })
            .WithName("GetCart");

            group.MapPost("/items", async (AddCartItemRequest request, ICartService cartService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await cartService.AddItemAsync(userId, request);
                return Results.Ok(result);
            })
            .WithName("AddCartItem");

            group.MapPut("/items/{id:guid}", async (Guid id, UpdateCartItemRequest request, ICartService cartService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await cartService.UpdateItemAsync(userId, id, request);
                return Results.Ok(result);
            })
            .WithName("UpdateCartItem");

            group.MapDelete("/items/{id:guid}", async (Guid id, ICartService cartService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await cartService.RemoveItemAsync(userId, id);
                return Results.Ok(result);
            })
            .WithName("RemoveCartItem");

            group.MapPost("/coupon", async (ApplyCouponRequest request, ICartService cartService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await cartService.ApplyCouponAsync(userId, request);
                return Results.Ok(result);
            })
            .WithName("ApplyCoupon");

            group.MapPost("/checkout", async (CheckoutRequest request, ICartService cartService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await cartService.CheckoutAsync(userId, request);
                return Results.Created($"/api/orders/{result.Id}", result);
            })
            .WithName("Checkout");
        }
    }
}
