using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;

namespace TestFirstProject.BackgroundServices;

/// <summary>
/// Background service that checks for low stock levels and logs alerts.
/// Runs every 6 hours.
/// </summary>
public class LowStockAlertService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LowStockAlertService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);

    public LowStockAlertService(IServiceScopeFactory scopeFactory, ILogger<LowStockAlertService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Low stock alert service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckLowStockAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during low stock check.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckLowStockAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PersonsContext>();

        var lowStockEntries = await context.StockEntries
            .Include(s => s.ProductVariant)
                .ThenInclude(v => v.Product)
            .Include(s => s.Warehouse)
            .Where(s => (s.QuantityOnHand - s.QuantityReserved) <= s.LowStockThreshold)
            .ToListAsync();

        if (lowStockEntries.Count == 0) return;

        _logger.LogWarning("Low stock alert: {Count} items below threshold.", lowStockEntries.Count);

        foreach (var entry in lowStockEntries)
        {
            var available = entry.QuantityOnHand - entry.QuantityReserved;
            _logger.LogWarning(
                "LOW STOCK: Product '{ProductName}' (SKU: {SKU}) at warehouse '{Warehouse}' — " +
                "Available: {Available}, Threshold: {Threshold}",
                entry.ProductVariant.Product.Name,
                entry.ProductVariant.SKU,
                entry.Warehouse.Name,
                available,
                entry.LowStockThreshold
            );
        }
    }
}
