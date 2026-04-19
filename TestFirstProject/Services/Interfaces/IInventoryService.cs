using TestFirstProject.DTOs;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.Services.Interfaces
{
    public interface IInventoryService
    {
        /// <summary>Reserve stock for a list of (variantId, quantity) pairs. Throws if insufficient.</summary>
        Task ReserveStockAsync(List<(Guid VariantId, int Quantity)> items, string referenceId);

        /// <summary>Release previously reserved stock (e.g. on cancellation).</summary>
        Task ReleaseStockAsync(List<(Guid VariantId, int Quantity)> items, string referenceId);

        /// <summary>Convert reserved stock to sold (on payment capture).</summary>
        Task ConfirmStockSoldAsync(List<(Guid VariantId, int Quantity)> items, string referenceId);

        /// <summary>Get available stock for a variant across all warehouses.</summary>
        Task<int> GetAvailableStockAsync(Guid variantId);

        // Admin
        Task<List<StockLevelResponse>> GetStockLevelsAsync();
        Task<RestockRequestResponse> CreateRestockRequestAsync(CreateRestockRequest request, Guid userId);
        Task<List<RestockRequestResponse>> GetRestockRequestsAsync();
        Task<List<StockLevelResponse>> GetLowStockItemsAsync();
    }
}
