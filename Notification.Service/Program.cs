using MassTransit;
using Notification.Service.Consumers;

var builder = Host.CreateApplicationBuilder(args);

// --- MassTransit with RabbitMQ and retry policy ---
builder.Services.AddMassTransit(x =>
{
    // Register the consumer
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("order-created-notifications", e =>
        {
            // Retry policy: 3 attempts with 5-second intervals
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
