using Contracts;
using MassTransit;

namespace Notification.Service.Consumers;

/// <summary>
/// Consumes OrderCreated events and logs a notification message to the console.
/// MassTransit retry policy (3 attempts with 5-second intervals) is configured in Program.cs.
/// </summary>
public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderCreated> context)
    {
        var order = context.Message;

        _logger.LogInformation(
            "Notification: New order received! " +
            "OrderId={OrderId}, Customer={CustomerName}, Product={ProductName}, " +
            "Quantity={Quantity}, Price={Price:C}, CreatedAt={CreatedAt}",
            order.OrderId,
            order.CustomerName,
            order.ProductName,
            order.Quantity,
            order.Price,
            order.CreatedAt);

        return Task.CompletedTask;
    }
}
