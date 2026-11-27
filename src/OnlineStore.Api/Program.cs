using Microsoft.EntityFrameworkCore;
using OnlineStore.Api.Dtos;
using OnlineStore.Api.Persistence;
using OnlineStore.Api.Storage;
using OnlineStore.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<OnlineStoreDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddSingleton<InMemoryOrderStore>();
builder.Services.AddSingleton<INotificationService>(_ =>
    new ConsoleNotificationService(message => Console.WriteLine($"[Notification] {message}")));
builder.Services.AddTransient<OrderApplicationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/orders", (OrderStatus? status, InMemoryOrderStore store) =>
{
    IReadOnlyList<OrderResponse> orders = store
        .GetOrders(status)
        .Select(OrderResponse.FromOrder)
        .ToList();

    return Results.Ok(orders);
});

app.MapGet("/orders/{id:guid}", (Guid id, InMemoryOrderStore store) =>
{
    Order? order = store.GetOrderById(id);
    return order is null
        ? Results.NotFound(new { error = $"Order '{id}' was not found." })
        : Results.Ok(OrderResponse.FromOrder(order));
});

app.MapPost("/orders", (CreateOrderRequest request, InMemoryOrderStore store) =>
{
    if (request.Items is null || request.Items.Count == 0)
    {
        return Results.BadRequest(new { error = "Order must contain at least one item." });
    }

    Customer? customer = store.GetCustomerById(request.CustomerId);
    if (customer is null)
    {
        return Results.BadRequest(new { error = $"Customer '{request.CustomerId}' was not found." });
    }

    var order = new Order(customer);
    try
    {
        foreach (CreateOrderItemRequest item in request.Items)
        {
            Product? product = store.GetProductById(item.ProductId);
            if (product is null)
            {
                return Results.BadRequest(new { error = $"Product '{item.ProductId}' was not found." });
            }

            order.AddItem(product, item.Quantity);
        }
    }
    catch (Exception ex) when (ex is ArgumentException or DomainRuleViolationException)
    {
        return Results.BadRequest(new { error = ex.Message });
    }

    store.TryAddOrder(order);
    OrderResponse response = OrderResponse.FromOrder(order);
    return Results.Created($"/orders/{order.Id}", response);
});

app.MapPost("/orders/{id:guid}/pay", (Guid id, InMemoryOrderStore store, OrderApplicationService orderService) =>
{
    Order? order = store.GetOrderById(id);
    if (order is null)
    {
        return Results.NotFound(new { error = $"Order '{id}' was not found." });
    }

    try
    {
        orderService.ProcessPayment(order, new CardPayment("4242"));
        return Results.Ok(OrderResponse.FromOrder(order));
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

app.Run();
