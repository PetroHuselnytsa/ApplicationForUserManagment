using Contracts;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// ── MassTransit / RabbitMQ ────────────────────────────────────────────────────
// Reads connection details from appsettings.json under "RabbitMq".
// No consumers are registered here — Order.API is a publisher only.
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq");

        cfg.Host(rabbitMqConfig["Host"], h =>
        {
            h.Username(rabbitMqConfig["Username"] ?? "guest");
            h.Password(rabbitMqConfig["Password"] ?? "guest");
        });
    });
});

var app = builder.Build();

// ── POST /api/orders ──────────────────────────────────────────────────────────
// Accepts a CreateOrderRequest, generates an OrderId, publishes an OrderCreated
// event to RabbitMQ via MassTransit, and returns 201 Created.
app.MapPost("/api/orders", async (CreateOrderRequest request, IPublishEndpoint publishEndpoint) =>
{
    // Basic validation
    if (request.CustomerId == Guid.Empty)
        return Results.BadRequest(new { error = "CustomerId is required." });

    if (request.TotalAmount <= 0)
        return Results.BadRequest(new { error = "TotalAmount must be greater than zero." });

    var orderId = Guid.NewGuid();
    var createdAt = DateTime.UtcNow;

    // Publish the OrderCreated event — MassTransit routes it to RabbitMQ.
    await publishEndpoint.Publish(new OrderCreated(
        OrderId: orderId,
        CustomerId: request.CustomerId,
        TotalAmount: request.TotalAmount,
        CreatedAt: createdAt));

    return Results.Created($"/api/orders/{orderId}", new
    {
        OrderId = orderId,
        request.CustomerId,
        request.TotalAmount,
        CreatedAt = createdAt
    });
});

app.Run();

// ── Request model ─────────────────────────────────────────────────────────────
/// <summary>Payload accepted by POST /api/orders.</summary>
/// <param name="CustomerId">ID of the customer placing the order.</param>
/// <param name="TotalAmount">Total monetary value of the order.</param>
public record CreateOrderRequest(Guid CustomerId, decimal TotalAmount);
