using MassTransit;
using Notification.Service.Consumers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        // ── MassTransit / RabbitMQ ────────────────────────────────────────────
        // Registers OrderCreatedConsumer with a 3-attempt retry policy.
        // "Attempts(3)" means: 1 initial attempt + 2 retries = 3 total deliveries.
        // On final failure, MassTransit automatically moves the message to the
        // fault queue (orderscreated_error) in RabbitMQ.
        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderCreatedConsumer>(cfg =>
            {
                // Immediate retry — re-delivers the message up to 3 times total
                // before the message is moved to the error queue.
                cfg.UseMessageRetry(r => r.Attempts(3));
            });

            x.UsingRabbitMq((busCtx, cfg) =>
            {
                var rabbitMqConfig = ctx.Configuration.GetSection("RabbitMq");

                cfg.Host(rabbitMqConfig["Host"], h =>
                {
                    h.Username(rabbitMqConfig["Username"] ?? "guest");
                    h.Password(rabbitMqConfig["Password"] ?? "guest");
                });

                // Auto-configure receive endpoints for all registered consumers.
                // This creates a durable queue named after the consumer (snake_case).
                cfg.ConfigureEndpoints(busCtx);
            });
        });
    })
    .Build();

await host.RunAsync();
