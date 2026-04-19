using System.Threading.Channels;
using TestFirstProject.DTOs.Notifications;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services
{
    /// <summary>
    /// Background hosted service that processes notification creation via a Channel queue.
    /// Domain events (e.g. new message received) enqueue notification requests,
    /// and this service processes them asynchronously to avoid blocking the main request.
    /// </summary>
    public class BackgroundNotificationService : BackgroundService
    {
        private readonly Channel<NotificationRequest> _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundNotificationService> _logger;

        public BackgroundNotificationService(
            Channel<NotificationRequest> channel,
            IServiceProvider serviceProvider,
            ILogger<BackgroundNotificationService> logger)
        {
            _channel = channel;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundNotificationService started.");

            await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var notificationService = scope.ServiceProvider
                        .GetRequiredService<INotificationService>();
                    var signalRNotifier = scope.ServiceProvider
                        .GetRequiredService<ISignalRNotifier>();

                    // Create the notification in the database
                    var notificationDto = await notificationService.CreateNotificationAsync(
                        request.UserId,
                        request.Type,
                        request.Title,
                        request.Body);

                    // Push via SignalR if the notification was created (not suppressed by preferences)
                    if (notificationDto != null)
                    {
                        await signalRNotifier.SendNotificationToUserAsync(
                            request.UserId, notificationDto);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing notification for user {UserId}", request.UserId);
                }
            }

            _logger.LogInformation("BackgroundNotificationService stopped.");
        }
    }

    /// <summary>
    /// Represents a queued notification request for background processing.
    /// </summary>
    public class NotificationRequest
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
    }

    /// <summary>
    /// Provides a simple interface for enqueuing notification work items.
    /// </summary>
    public interface INotificationQueue
    {
        ValueTask EnqueueAsync(NotificationRequest request, CancellationToken cancellationToken = default);
    }

    public class NotificationQueue : INotificationQueue
    {
        private readonly Channel<NotificationRequest> _channel;

        public NotificationQueue(Channel<NotificationRequest> channel)
        {
            _channel = channel;
        }

        public async ValueTask EnqueueAsync(NotificationRequest request, CancellationToken cancellationToken = default)
        {
            await _channel.Writer.WriteAsync(request, cancellationToken);
        }
    }
}
