using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;

namespace TestFirstProject.BackgroundJobs
{
    /// <summary>
    /// Background job that removes carts inactive for more than 30 days.
    /// Runs every 6 hours.
    /// </summary>
    public class CartExpiryJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CartExpiryJob> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(6);
        private readonly TimeSpan _expiryThreshold = TimeSpan.FromDays(30);

        public CartExpiryJob(IServiceProvider serviceProvider, ILogger<CartExpiryJob> logger)
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
                    await CleanExpiredCartsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning expired carts.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task CleanExpiredCartsAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PersonsContext>();

            var cutoff = DateTime.UtcNow - _expiryThreshold;
            var expiredCarts = await context.Carts
                .Include(c => c.Items)
                .Where(c => c.LastActivityAt < cutoff)
                .ToListAsync(ct);

            if (expiredCarts.Count == 0) return;

            _logger.LogInformation("Removing {Count} expired carts.", expiredCarts.Count);

            foreach (var cart in expiredCarts)
            {
                context.CartItems.RemoveRange(cart.Items);
            }
            context.Carts.RemoveRange(expiredCarts);

            await context.SaveChangesAsync(ct);
        }
    }
}
