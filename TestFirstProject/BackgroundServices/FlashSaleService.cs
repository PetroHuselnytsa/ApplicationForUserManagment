using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;

namespace TestFirstProject.BackgroundServices;

/// <summary>
/// Background service that activates and deactivates flash sales based on their time windows.
/// Runs every 5 minutes.
/// </summary>
public class FlashSaleService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FlashSaleService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

    public FlashSaleService(IServiceScopeFactory scopeFactory, ILogger<FlashSaleService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Flash sale activation service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateFlashSaleStatusesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during flash sale status update.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task UpdateFlashSaleStatusesAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PersonsContext>();

        var now = DateTime.UtcNow;

        // Activate flash sales that should be active
        var toActivate = await context.FlashSales
            .Where(f => !f.IsActive && f.StartTime <= now && f.EndTime > now)
            .ToListAsync();

        foreach (var sale in toActivate)
        {
            sale.IsActive = true;
            _logger.LogInformation("Flash sale {SaleId} activated for product {ProductId}.", sale.Id, sale.ProductId);
        }

        // Deactivate expired flash sales
        var toDeactivate = await context.FlashSales
            .Where(f => f.IsActive && f.EndTime <= now)
            .ToListAsync();

        foreach (var sale in toDeactivate)
        {
            sale.IsActive = false;
            _logger.LogInformation("Flash sale {SaleId} deactivated (expired).", sale.Id);
        }

        if (toActivate.Count > 0 || toDeactivate.Count > 0)
        {
            await context.SaveChangesAsync();
        }
    }
}
