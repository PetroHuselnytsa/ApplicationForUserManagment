using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    public interface IOrderService
    {
        Task<PaginatedResponse<OrderListResponse>> GetUserOrdersAsync(Guid userId, int page, int pageSize);
        Task<OrderResponse> GetOrderByIdAsync(Guid orderId, Guid userId);
        Task<OrderResponse> CancelOrderAsync(Guid orderId, Guid userId);
        Task<TrackingResponse> GetTrackingAsync(Guid orderId, Guid userId);

        // Admin
        Task<PaginatedResponse<OrderListResponse>> GetAllOrdersAsync(int page, int pageSize, string? status);
        Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, Guid adminUserId);
        Task<SalesReportResponse> GetSalesReportAsync(string? period, DateTime? startDate, DateTime? endDate);
    }
}
