using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TestFirstProject.Settings;

namespace TestFirstProject.TelegramBot
{
    /// <summary>
    /// Background service that starts long polling for the Telegram bot.
    /// Runs concurrently with the ASP.NET Core web host.
    /// </summary>
    public class BotHostedService : BackgroundService
    {
        private readonly ITelegramBotClient _bot;
        private readonly BotUpdateHandler _handler;
        private readonly ILogger<BotHostedService> _logger;

        public BotHostedService(
            ITelegramBotClient bot,
            BotUpdateHandler handler,
            ILogger<BotHostedService> logger)
        {
            _bot = bot;
            _handler = handler;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Telegram bot long polling started.");

            var receiverOptions = new ReceiverOptions
            {
                // Discard any updates that arrived while the bot was offline.
                AllowedUpdates = Array.Empty<UpdateType>(),
                DropPendingUpdates = true
            };

            // ReceiveAsync blocks until the cancellation token is signalled.
            await _bot.ReceiveAsync(
                updateHandler: _handler,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken);

            _logger.LogInformation("Telegram bot long polling stopped.");
        }
    }
}
