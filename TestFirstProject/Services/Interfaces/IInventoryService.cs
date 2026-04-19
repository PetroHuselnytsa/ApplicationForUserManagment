using TestFirstProject.DTOs.Admin;
using TestFirstProject.DTOs.Common;
using TestFirstProject.Models;

namespace TestFirstProject.Services.Interfaces;

/// <summary>
/// All inventory changes MUST go through this service — never update stock directly.
/// </summary>
public interface IInventoryService
{
    Task<List<StockLevelDto>> GetStockLevelsAsync(Guid? productVariantId = null, Guid? warehouseId = null);
    Task ReserveStockAsync(Guid productVariantId, int quantity, Guid orderId, Guid userId);
    Task ReleaseStockAsync(Guid productVariantId, int quantity, Guid orderId, Guid userId);
    Task ConfirmSaleAsync(Guid productVariantId, int quantity, Guid orderId, Guid userId);
    Task<int> GetAvailableStockAsync(Guid productVariantId);
    Task<List<StockLevelDto>> GetLowStockItemsAsync();
    Task<RestockRequestDto> CreateRestockRequestAsync(CreateRestockRequest request, Guid userId);
    Task<List<RestockRequestDto>> GetRestockRequestsAsync();
}
