using System.Collections.Concurrent;
using Contracts;
using MassTransit;
using Order.API.DTOs;

var builder = WebApplication.CreateBuilder(args);

// --- MassTransit with RabbitMQ ---
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// In-memory order store
var orders = new ConcurrentDictionary<Guid, Order.API.Models.Order>();

// --- POST /api/orders — Create a new order and publish OrderCreated event ---
app.MapPost("/api/orders", async (CreateOrderRequest request, IPublishEndpoint publishEndpoint) =>
{
    var order = new Order.API.Models.Order
    {
        Id = Guid.NewGuid(),
        CustomerName = request.CustomerName,
        ProductName = request.ProductName,
        Quantity = request.Quantity,
        Price = request.Price,
        CreatedAt = DateTime.UtcNow
    };

    orders[order.Id] = order;

    // Publish the OrderCreated event to RabbitMQ via MassTransit
    await publishEndpoint.Publish(new OrderCreated
    {
        OrderId = order.Id,
        CustomerName = order.CustomerName,
        ProductName = order.ProductName,
        Quantity = order.Quantity,
        Price = order.Price,
        CreatedAt = order.CreatedAt
    });

    var response = new CreateOrderResponse
    {
        OrderId = order.Id,
        CustomerName = order.CustomerName,
        ProductName = order.ProductName,
        Quantity = order.Quantity,
        Price = order.Price,
        CreatedAt = order.CreatedAt
    };

    return Results.Created($"/api/orders/{order.Id}", response);
});

// --- GET /api/orders — List all orders (convenience endpoint) ---
app.MapGet("/api/orders", () =>
{
    return Results.Ok(orders.Values.ToList());
});

// --- GET /api/orders/{id} — Get a single order by ID ---
app.MapGet("/api/orders/{id:guid}", (Guid id) =>
{
    return orders.TryGetValue(id, out var order)
        ? Results.Ok(order)
        : Results.NotFound(new { Message = $"Order with ID {id} not found." });
});

app.Run();
