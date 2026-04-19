using TestFirstProject.DTOs.Cart;
using TestFirstProject.DTOs.Orders;

namespace TestFirstProject.Services.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid userId);
    Task<CartDto> AddItemAsync(Guid userId, AddCartItemRequest request);
    Task<CartDto> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request);
    Task<CartDto> RemoveItemAsync(Guid userId, Guid cartItemId);
    Task<CartDto> ApplyCouponAsync(Guid userId, ApplyCouponRequest request);
    Task<OrderDetailDto> CheckoutAsync(Guid userId, CheckoutRequest request);
}
