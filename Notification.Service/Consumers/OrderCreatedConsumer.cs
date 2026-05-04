using Contracts;
using MassTransit;

namespace Notification.Service.Consumers;

/// <summary>
/// Consumes <see cref="OrderCreated"/> events published by Order.API.
/// Logs the event details to the console (stdout / structured logging).
///
/// Retry policy is configured during consumer registration in Program.cs:
/// 3 total attempts (initial attempt + 2 automatic retries on failure).
/// After all attempts are exhausted MassTransit moves the message to the
/// corresponding *_error queue in RabbitMQ automatically.
/// </summary>
public sealed class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderCreated> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[Notification] Order {OrderId} created for customer {CustomerId} " +
            "with total {TotalAmount:C} at {CreatedAt:O}.",
            msg.OrderId,
            msg.CustomerId,
            msg.TotalAmount,
            msg.CreatedAt);

        return Task.CompletedTask;
    }
}
