using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class InventoryService : IInventoryService
    {
        private readonly PersonsContext _context;

        public InventoryService(PersonsContext context)
        {
            _context = context;
        }

        public async Task ReserveStockAsync(List<(Guid VariantId, int Quantity)> items, string referenceId)
        {
            await AdjustStockAsync(items, referenceId, InventoryTransactionType.Reserved, (entry, qty) =>
            {
                int available = entry.QuantityOnHand - entry.QuantityReserved;
                if (available <= 0) return 0;
                int toReserve = Math.Min(available, qty);
                entry.QuantityReserved += toReserve;
                return toReserve;
            }, throwOnShortfall: true);
        }

        public async Task ReleaseStockAsync(List<(Guid VariantId, int Quantity)> items, string referenceId)
        {
            await AdjustStockAsync(items, referenceId, InventoryTransactionType.Released, (entry, qty) =>
            {
                if (entry.QuantityReserved <= 0) return 0;
                int toRelease = Math.Min(entry.QuantityReserved, qty);
                entry.QuantityReserved -= toRelease;
                return toRelease;
            }, throwOnShortfall: false);
        }

        public async Task ConfirmStockSoldAsync(List<(Guid VariantId, int Quantity)> items, string referenceId)
        {
            await AdjustStockAsync(items, referenceId, InventoryTransactionType.Sold, (entry, qty) =>
            {
                if (entry.QuantityReserved <= 0) return 0;
                int toSell = Math.Min(entry.QuantityReserved, qty);
                entry.QuantityOnHand -= toSell;
                entry.QuantityReserved -= toSell;
                return toSell;
            }, throwOnShortfall: false);
        }

        public async Task<int> GetAvailableStockAsync(Guid variantId)
        {
            return await _context.StockEntries
                .Where(se => se.ProductVariantId == variantId)
                .SumAsync(se => se.QuantityOnHand - se.QuantityReserved);
        }

        public async Task<List<StockLevelResponse>> GetStockLevelsAsync()
        {
            return await BuildStockLevelQuery().ToListAsync();
        }

        public async Task<List<StockLevelResponse>> GetLowStockItemsAsync()
        {
            return await BuildStockLevelQuery()
                .Where(r => r.IsLowStock)
                .ToListAsync();
        }

        public async Task<RestockRequestResponse> CreateRestockRequestAsync(CreateRestockRequest request, Guid userId)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(pv => pv.Id == request.ProductVariantId)
                ?? throw new NotFoundException($"Product variant {request.ProductVariantId} not found");

            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == request.WarehouseId)
                ?? throw new NotFoundException($"Warehouse {request.WarehouseId} not found");

            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new NotFoundException($"User {userId} not found");

            var restockRequest = new RestockRequest
            {
                Id = Guid.NewGuid(),
                ProductVariantId = request.ProductVariantId,
                WarehouseId = request.WarehouseId,
                RequestedQuantity = request.Quantity,
                Status = RestockRequestStatus.Pending,
                RequestedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.RestockRequests.Add(restockRequest);
            await _context.SaveChangesAsync();

            return new RestockRequestResponse(
                restockRequest.Id,
                variant.Sku,
                warehouse.Name,
                restockRequest.RequestedQuantity,
                restockRequest.Status.ToString(),
                user.Username,
                restockRequest.CreatedAt,
                restockRequest.FulfilledAt);
        }

        public async Task<List<RestockRequestResponse>> GetRestockRequestsAsync()
        {
            return await _context.RestockRequests
                .Include(rr => rr.ProductVariant)
                .Include(rr => rr.Warehouse)
                .Include(rr => rr.RequestedByUser)
                .OrderByDescending(rr => rr.CreatedAt)
                .Select(rr => new RestockRequestResponse(
                    rr.Id,
                    rr.ProductVariant.Sku,
                    rr.Warehouse.Name,
                    rr.RequestedQuantity,
                    rr.Status.ToString(),
                    rr.RequestedByUser.Username,
                    rr.CreatedAt,
                    rr.FulfilledAt))
                .ToListAsync();
        }

        // --- Private Helpers ---

        /// <summary>
        /// Unified stock adjustment loop. Fetches all stock entries for the given variants in one query,
        /// then distributes quantities using the provided mutation delegate.
        /// </summary>
        private async Task AdjustStockAsync(
            List<(Guid VariantId, int Quantity)> items,
            string referenceId,
            InventoryTransactionType transactionType,
            Func<StockEntry, int, int> mutate,
            bool throwOnShortfall)
        {
            var variantIds = items.Select(i => i.VariantId).Distinct().ToList();

            var allStockEntries = await _context.StockEntries
                .Where(se => variantIds.Contains(se.ProductVariantId))
                .ToListAsync();

            var entriesByVariant = allStockEntries
                .GroupBy(se => se.ProductVariantId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(se => se.QuantityOnHand - se.QuantityReserved).ToList());

            foreach (var (variantId, quantity) in items)
            {
                int remaining = quantity;

                if (entriesByVariant.TryGetValue(variantId, out var stockEntries))
                {
                    foreach (var entry in stockEntries)
                    {
                        if (remaining <= 0) break;

                        int adjusted = mutate(entry, remaining);
                        if (adjusted <= 0) continue;

                        remaining -= adjusted;

                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            Id = Guid.NewGuid(),
                            StockEntryId = entry.Id,
                            Type = transactionType,
                            QuantityChange = adjusted,
                            Reason = $"Stock {transactionType.ToString().ToLower()}",
                            ReferenceId = referenceId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                if (remaining > 0 && throwOnShortfall)
                {
                    throw new AppException($"Insufficient stock for variant {variantId}.");
                }
            }

            await _context.SaveChangesAsync();
        }

        private IQueryable<StockLevelResponse> BuildStockLevelQuery()
        {
            return _context.StockEntries
                .Include(se => se.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(se => se.Warehouse)
                .Select(se => new StockLevelResponse(
                    se.ProductVariantId,
                    se.ProductVariant.Sku,
                    se.ProductVariant.Product.Name,
                    se.Warehouse.Name,
                    se.QuantityOnHand,
                    se.QuantityReserved,
                    se.QuantityOnHand - se.QuantityReserved,
                    se.LowStockThreshold,
                    (se.QuantityOnHand - se.QuantityReserved) <= se.LowStockThreshold));
        }
    }
}
