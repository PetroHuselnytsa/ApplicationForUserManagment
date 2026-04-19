using TestFirstProject.DTOs.Cart;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/cart")
            .RequireAuthorization();

        // GET /api/cart — get current user's cart
        group.MapGet("/", async (ICartService cartService, HttpContext context) =>
        {
            var userId = context.GetUserId();
            var cart = await cartService.GetCartAsync(userId);
            return Results.Ok(cart);
        });

        // POST /api/cart/items — add item to cart
        group.MapPost("/items", async (AddCartItemRequest request, ICartService cartService, HttpContext context) =>
        {
            var userId = context.GetUserId();
            var cart = await cartService.AddItemAsync(userId, request);
            return Results.Ok(cart);
        });

        // PUT /api/cart/items/{id} — update item quantity
        group.MapPut("/items/{id:guid}", async (Guid id, UpdateCartItemRequest request, ICartService cartService, HttpContext context) =>
        {
            var userId = context.GetUserId();
            var cart = await cartService.UpdateItemQuantityAsync(userId, id, request);
            return Results.Ok(cart);
        });

        // DELETE /api/cart/items/{id} — remove item from cart
        group.MapDelete("/items/{id:guid}", async (Guid id, ICartService cartService, HttpContext context) =>
        {
            var userId = context.GetUserId();
            var cart = await cartService.RemoveItemAsync(userId, id);
            return Results.Ok(cart);
        });

        // POST /api/cart/coupon — apply coupon code
        group.MapPost("/coupon", async (ApplyCouponRequest request, ICartService cartService, HttpContext context) =>
        {
            var userId = context.GetUserId();
            var cart = await cartService.ApplyCouponAsync(userId, request);
            return Results.Ok(cart);
        });

        // POST /api/cart/checkout — convert cart to order
        group.MapPost("/checkout", async (CheckoutRequest request, ICartService cartService, HttpContext context) =>
        {
            var userId = context.GetUserId();
            var order = await cartService.CheckoutAsync(userId, request);
            return Results.Created($"/api/orders/{order.Id}", order);
        });
    }
}
