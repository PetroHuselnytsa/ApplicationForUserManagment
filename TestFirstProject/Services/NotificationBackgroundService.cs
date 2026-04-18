using System.Threading.Channels;
using TestFirstProject.DTOs;
using TestFirstProject.Models;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Event representing a notification that needs to be created and pushed in the background.
    /// </summary>
    public record NotificationEvent(
        Guid UserId,
        NotificationType Type,
        string Title,
        string Body,
        string? ReferenceId = null
    );

    /// <summary>
    /// Channel-based background service that processes notification events asynchronously.
    /// This decouples notification creation from the request pipeline, improving response times.
    /// </summary>
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly Channel<NotificationEvent> _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(
            Channel<NotificationEvent> channel,
            IServiceProvider serviceProvider,
            ILogger<NotificationBackgroundService> logger)
        {
            _channel = channel;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationBackgroundService started.");

            await foreach (var evt in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    // Create a new scope for each event to get fresh scoped services
                    using var scope = _serviceProvider.CreateScope();
                    var notificationService = scope.ServiceProvider
                        .GetRequiredService<INotificationService>();
                    var signalRNotifier = scope.ServiceProvider
                        .GetRequiredService<ISignalRNotifier>();

                    // Create the notification
                    var notification = await notificationService.CreateNotificationAsync(
                        evt.UserId, evt.Type, evt.Title, evt.Body, evt.ReferenceId);

                    // Push via SignalR if the notification was created (not filtered by preferences)
                    if (notification != null)
                    {
                        await signalRNotifier.NotifyNewNotificationAsync(evt.UserId, notification);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing notification event for user {UserId}: {Type}",
                        evt.UserId, evt.Type);
                }
            }

            _logger.LogInformation("NotificationBackgroundService stopped.");
        }
    }

    /// <summary>
    /// Extension to conveniently publish notification events from anywhere in the application.
    /// </summary>
    public static class NotificationChannelExtensions
    {
        /// <summary>
        /// Publishes a notification event to the background processing channel.
        /// </summary>
        public static async Task PublishNotificationAsync(
            this Channel<NotificationEvent> channel,
            Guid userId,
            NotificationType type,
            string title,
            string body,
            string? referenceId = null)
        {
            await channel.Writer.WriteAsync(new NotificationEvent(
                userId, type, title, body, referenceId));
        }
    }
}
