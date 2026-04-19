using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;

namespace TestFirstProject.BackgroundServices;

/// <summary>
/// Background service that cleans up expired carts (inactive for 30+ days).
/// Runs every 24 hours.
/// </summary>
public class CartCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CartCleanupService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);
    private static readonly TimeSpan CartExpiry = TimeSpan.FromDays(30);

    public CartCleanupService(IServiceScopeFactory scopeFactory, ILogger<CartCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cart cleanup service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredCartsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cart cleanup.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredCartsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PersonsContext>();

        var cutoff = DateTime.UtcNow - CartExpiry;

        var expiredCarts = await context.Carts
            .Include(c => c.Items)
            .Where(c => c.LastActivityAt < cutoff)
            .ToListAsync();

        if (expiredCarts.Count == 0) return;

        _logger.LogInformation("Cleaning up {Count} expired carts.", expiredCarts.Count);

        foreach (var cart in expiredCarts)
        {
            context.CartItems.RemoveRange(cart.Items);
            context.Carts.Remove(cart);
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("Expired cart cleanup complete.");
    }
}
