using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;

namespace TestFirstProject.BackgroundJobs
{
    /// <summary>
    /// Background job that checks for low-stock items and logs alerts.
    /// Runs every hour.
    /// </summary>
    public class LowStockAlertJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LowStockAlertJob> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);

        public LowStockAlertJob(IServiceProvider serviceProvider, ILogger<LowStockAlertJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckLowStockAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking low stock.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task CheckLowStockAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PersonsContext>();

            var lowStockEntries = await context.StockEntries
                .Include(s => s.ProductVariant)
                    .ThenInclude(v => v.Product)
                .Include(s => s.Warehouse)
                .Where(s => (s.QuantityOnHand - s.QuantityReserved) <= s.LowStockThreshold)
                .ToListAsync(ct);

            foreach (var entry in lowStockEntries)
            {
                var available = entry.QuantityOnHand - entry.QuantityReserved;
                _logger.LogWarning(
                    "LOW STOCK ALERT: {Product} ({Sku}) at {Warehouse} — {Available} available (threshold: {Threshold})",
                    entry.ProductVariant.Product.Name,
                    entry.ProductVariant.Sku,
                    entry.Warehouse.Name,
                    available,
                    entry.LowStockThreshold);
            }
        }
    }
}
