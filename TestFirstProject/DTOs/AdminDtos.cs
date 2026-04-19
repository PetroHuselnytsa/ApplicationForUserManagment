namespace TestFirstProject.DTOs
{
    // --- Admin / Inventory ---

    public record StockLevelResponse(
        Guid ProductVariantId,
        string VariantSku,
        string ProductName,
        string WarehouseName,
        int QuantityOnHand,
        int QuantityReserved,
        int Available,
        int LowStockThreshold,
        bool IsLowStock);

    public record CreateRestockRequest(
        Guid ProductVariantId,
        Guid WarehouseId,
        int Quantity);

    public record RestockRequestResponse(
        Guid Id,
        string VariantSku,
        string WarehouseName,
        int RequestedQuantity,
        string Status,
        string RequestedBy,
        DateTime CreatedAt,
        DateTime? FulfilledAt);

    public record SalesReportResponse(
        DateTime PeriodStart,
        DateTime PeriodEnd,
        decimal TotalRevenue,
        int TotalOrders,
        int TotalItemsSold,
        decimal AverageOrderValue,
        List<TopProductResponse> TopProducts);

    public record TopProductResponse(
        string ProductName,
        int QuantitySold,
        decimal Revenue);
}
