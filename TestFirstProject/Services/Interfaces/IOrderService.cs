using TestFirstProject.DTOs.Common;
using TestFirstProject.DTOs.Orders;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.Services.Interfaces;

public interface IOrderService
{
    Task<PaginatedResult<OrderListDto>> GetUserOrdersAsync(Guid userId, int page, int pageSize);
    Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId, Guid userId);
    Task<OrderDetailDto> CancelOrderAsync(Guid orderId, Guid userId);
    Task<OrderTrackingDto> GetTrackingInfoAsync(Guid orderId, Guid userId);

    // Admin operations
    Task<PaginatedResult<OrderListDto>> GetAllOrdersAsync(AdminOrderFilterRequest filter);
    Task<OrderDetailDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, Guid actorUserId, string? notes);
    Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to);
}
