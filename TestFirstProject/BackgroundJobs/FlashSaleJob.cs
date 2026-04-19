using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;

namespace TestFirstProject.BackgroundJobs
{
    /// <summary>
    /// Background job that activates/deactivates flash sales based on their time windows.
    /// Runs every 5 minutes.
    /// </summary>
    public class FlashSaleJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FlashSaleJob> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public FlashSaleJob(IServiceProvider serviceProvider, ILogger<FlashSaleJob> logger)
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
                    await ManageFlashSalesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error managing flash sales.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task ManageFlashSalesAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PersonsContext>();
            var now = DateTime.UtcNow;

            // Activate flash sales that should now be active
            var toActivate = await context.FlashSales
                .Where(f => !f.IsActive && f.StartsAt <= now && f.EndsAt > now)
                .ToListAsync(ct);

            foreach (var sale in toActivate)
            {
                sale.IsActive = true;
                _logger.LogInformation("Activated flash sale {Id} for product {ProductId}.", sale.Id, sale.ProductId);
            }

            // Deactivate flash sales that have expired
            var toDeactivate = await context.FlashSales
                .Where(f => f.IsActive && f.EndsAt <= now)
                .ToListAsync(ct);

            foreach (var sale in toDeactivate)
            {
                sale.IsActive = false;
                _logger.LogInformation("Deactivated flash sale {Id} for product {ProductId}.", sale.Id, sale.ProductId);
            }

            if (toActivate.Count > 0 || toDeactivate.Count > 0)
            {
                await context.SaveChangesAsync(ct);
            }
        }
    }
}
