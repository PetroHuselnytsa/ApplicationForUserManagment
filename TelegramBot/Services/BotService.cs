using Telegram.Bot;
using Telegram.Bot.Polling;

namespace TelegramBot.Services;

/// <summary>
/// Background service that manages the Telegram bot's long polling lifecycle.
/// Creates a new service scope per polling iteration to leverage scoped services.
/// </summary>
public class BotService(
    IServiceProvider serviceProvider,
    ILogger<BotService> logger) : BackgroundService
{
    /// <summary>
    /// Starts the long polling loop. On each iteration, creates a new scope
    /// to get fresh instances of scoped services, then starts receiving updates.
    /// Retries with a cooldown on failure.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Bot polling service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
                var updateHandler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();

                var receiverOptions = new ReceiverOptions
                {
                    DropPendingUpdates = true,
                    AllowedUpdates = [] // Receive all update types
                };

                var me = await botClient.GetMe(stoppingToken);
                logger.LogInformation("Bot started receiving updates as @{BotName}", me.Username);

                // Start long polling — blocks until cancellation or error
                await botClient.ReceiveAsync(
                    updateHandler: updateHandler,
                    receiverOptions: receiverOptions,
                    cancellationToken: stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown
                logger.LogInformation("Bot polling service is stopping");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Bot polling failed, restarting in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
