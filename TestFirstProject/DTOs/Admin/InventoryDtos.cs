using TestFirstProject.Models.Enums;

namespace TestFirstProject.DTOs.Admin;

// --- Request DTOs ---

public record CreateRestockRequest(
    Guid ProductVariantId,
    Guid WarehouseId,
    int Quantity
);

// --- Response DTOs ---

public record StockLevelDto(
    Guid ProductVariantId,
    string ProductName,
    string VariantSKU,
    string? Size,
    string? Color,
    Guid WarehouseId,
    string WarehouseName,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableQuantity,
    int LowStockThreshold,
    bool IsLowStock
);

public record RestockRequestDto(
    Guid Id,
    Guid ProductVariantId,
    string VariantSKU,
    Guid WarehouseId,
    string WarehouseName,
    int RequestedQuantity,
    int FulfilledQuantity,
    RestockRequestStatus Status,
    DateTime CreatedAt,
    DateTime? FulfilledAt
);
