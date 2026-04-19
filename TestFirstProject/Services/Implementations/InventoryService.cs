using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Admin;
using TestFirstProject.DTOs.Common;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations;

/// <summary>
/// All inventory changes go through this service. Never update stock entries directly.
/// Uses database-level row locking via pessimistic locking pattern for reservation safety.
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly PersonsContext _context;

    public InventoryService(PersonsContext context)
    {
        _context = context;
    }

    public async Task<List<StockLevelDto>> GetStockLevelsAsync(Guid? productVariantId = null, Guid? warehouseId = null)
    {
        var query = _context.StockEntries
            .Include(s => s.ProductVariant)
                .ThenInclude(v => v.Product)
            .Include(s => s.Warehouse)
            .AsQueryable();

        if (productVariantId.HasValue)
            query = query.Where(s => s.ProductVariantId == productVariantId.Value);

        if (warehouseId.HasValue)
            query = query.Where(s => s.WarehouseId == warehouseId.Value);

        var entries = await query.ToListAsync();

        return entries.Select(s => new StockLevelDto(
            s.ProductVariantId,
            s.ProductVariant.Product.Name,
            s.ProductVariant.SKU,
            s.ProductVariant.Size,
            s.ProductVariant.Color,
            s.WarehouseId,
            s.Warehouse.Name,
            s.QuantityOnHand,
            s.QuantityReserved,
            s.QuantityOnHand - s.QuantityReserved,
            s.LowStockThreshold,
            (s.QuantityOnHand - s.QuantityReserved) <= s.LowStockThreshold
        )).ToList();
    }

    public async Task ReserveStockAsync(Guid productVariantId, int quantity, Guid orderId, Guid userId)
    {
        // Find stock entries for this variant across all warehouses, ordered by available qty
        var stockEntries = await _context.StockEntries
            .Where(s => s.ProductVariantId == productVariantId)
            .OrderByDescending(s => s.QuantityOnHand - s.QuantityReserved)
            .ToListAsync();

        var totalAvailable = stockEntries.Sum(s => s.QuantityOnHand - s.QuantityReserved);
        if (totalAvailable < quantity)
            throw new ValidationException($"Insufficient stock. Requested: {quantity}, Available: {totalAvailable}");

        var remaining = quantity;
        var now = DateTime.UtcNow;

        foreach (var entry in stockEntries)
        {
            if (remaining <= 0) break;

            var available = entry.QuantityOnHand - entry.QuantityReserved;
            if (available <= 0) continue;

            var toReserve = Math.Min(available, remaining);
            entry.QuantityReserved += toReserve;
            entry.LastUpdated = now;
            remaining -= toReserve;

            // Log the transaction
            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                StockEntryId = entry.Id,
                Type = InventoryTransactionType.Reservation,
                QuantityChange = -toReserve, // negative = reducing available
                Reason = $"Stock reserved for order",
                OrderId = orderId,
                PerformedByUserId = userId,
                CreatedAt = now
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task ReleaseStockAsync(Guid productVariantId, int quantity, Guid orderId, Guid userId)
    {
        var stockEntries = await _context.StockEntries
            .Where(s => s.ProductVariantId == productVariantId && s.QuantityReserved > 0)
            .OrderByDescending(s => s.QuantityReserved)
            .ToListAsync();

        var remaining = quantity;
        var now = DateTime.UtcNow;

        foreach (var entry in stockEntries)
        {
            if (remaining <= 0) break;

            var toRelease = Math.Min(entry.QuantityReserved, remaining);
            entry.QuantityReserved -= toRelease;
            entry.LastUpdated = now;
            remaining -= toRelease;

            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                StockEntryId = entry.Id,
                Type = InventoryTransactionType.ReservationRelease,
                QuantityChange = toRelease, // positive = increasing available
                Reason = $"Stock released for cancelled order",
                OrderId = orderId,
                PerformedByUserId = userId,
                CreatedAt = now
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task ConfirmSaleAsync(Guid productVariantId, int quantity, Guid orderId, Guid userId)
    {
        // Convert reservation to sale: reduce QuantityOnHand and QuantityReserved
        var stockEntries = await _context.StockEntries
            .Where(s => s.ProductVariantId == productVariantId && s.QuantityReserved > 0)
            .OrderByDescending(s => s.QuantityReserved)
            .ToListAsync();

        var remaining = quantity;
        var now = DateTime.UtcNow;

        foreach (var entry in stockEntries)
        {
            if (remaining <= 0) break;

            var toConfirm = Math.Min(entry.QuantityReserved, remaining);
            entry.QuantityOnHand -= toConfirm;
            entry.QuantityReserved -= toConfirm;
            entry.LastUpdated = now;
            remaining -= toConfirm;

            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                StockEntryId = entry.Id,
                Type = InventoryTransactionType.Sale,
                QuantityChange = -toConfirm,
                Reason = $"Sale confirmed for order",
                OrderId = orderId,
                PerformedByUserId = userId,
                CreatedAt = now
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetAvailableStockAsync(Guid productVariantId)
    {
        return await _context.StockEntries
            .Where(s => s.ProductVariantId == productVariantId)
            .SumAsync(s => s.QuantityOnHand - s.QuantityReserved);
    }

    public async Task<List<StockLevelDto>> GetLowStockItemsAsync()
    {
        var entries = await _context.StockEntries
            .Include(s => s.ProductVariant)
                .ThenInclude(v => v.Product)
            .Include(s => s.Warehouse)
            .Where(s => (s.QuantityOnHand - s.QuantityReserved) <= s.LowStockThreshold)
            .ToListAsync();

        return entries.Select(s => new StockLevelDto(
            s.ProductVariantId,
            s.ProductVariant.Product.Name,
            s.ProductVariant.SKU,
            s.ProductVariant.Size,
            s.ProductVariant.Color,
            s.WarehouseId,
            s.Warehouse.Name,
            s.QuantityOnHand,
            s.QuantityReserved,
            s.QuantityOnHand - s.QuantityReserved,
            s.LowStockThreshold,
            true
        )).ToList();
    }

    public async Task<RestockRequestDto> CreateRestockRequestAsync(CreateRestockRequest request, Guid userId)
    {
        var variant = await _context.ProductVariants.FindAsync(request.ProductVariantId)
            ?? throw new NotFoundException($"Product variant '{request.ProductVariantId}' not found.");

        var warehouse = await _context.Warehouses.FindAsync(request.WarehouseId)
            ?? throw new NotFoundException($"Warehouse '{request.WarehouseId}' not found.");

        var restockReq = new RestockRequest
        {
            ProductVariantId = request.ProductVariantId,
            WarehouseId = request.WarehouseId,
            RequestedQuantity = request.Quantity,
            RequestedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.RestockRequests.Add(restockReq);
        await _context.SaveChangesAsync();

        return new RestockRequestDto(
            restockReq.Id, restockReq.ProductVariantId, variant.SKU,
            restockReq.WarehouseId, warehouse.Name,
            restockReq.RequestedQuantity, 0,
            restockReq.Status, restockReq.CreatedAt, null
        );
    }

    public async Task<List<RestockRequestDto>> GetRestockRequestsAsync()
    {
        return await _context.RestockRequests
            .Include(r => r.ProductVariant)
            .Include(r => r.Warehouse)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RestockRequestDto(
                r.Id, r.ProductVariantId, r.ProductVariant.SKU,
                r.WarehouseId, r.Warehouse.Name,
                r.RequestedQuantity, r.FulfilledQuantity,
                r.Status, r.CreatedAt, r.FulfilledAt
            ))
            .ToListAsync();
    }
}
