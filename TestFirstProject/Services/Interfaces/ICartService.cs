using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartResponse> GetCartAsync(Guid userId);
        Task<CartResponse> AddItemAsync(Guid userId, AddCartItemRequest request);
        Task<CartResponse> UpdateItemAsync(Guid userId, Guid itemId, UpdateCartItemRequest request);
        Task<CartResponse> RemoveItemAsync(Guid userId, Guid itemId);
        Task<CartResponse> ApplyCouponAsync(Guid userId, ApplyCouponRequest request);
        Task<OrderResponse> CheckoutAsync(Guid userId, CheckoutRequest request);
    }
}
