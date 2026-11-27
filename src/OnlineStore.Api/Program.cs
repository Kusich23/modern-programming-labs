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

using (IServiceScope scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OnlineStoreDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/orders", async (
    OrderStatus? status,
    OnlineStoreDbContext db,
    InMemoryOrderStore catalog,
    CancellationToken ct) =>
{
    IQueryable<OrderRow> query = db.Orders
        .AsNoTracking()
        .Include(o => o.Lines);

    if (status.HasValue)
    {
        string statusText = status.Value.ToString();
        query = query.Where(o => o.Status == statusText);
    }

    List<OrderRow> rows = await query
        .OrderBy(o => o.Id)
        .ToListAsync(ct);

    List<OrderResponse> response = rows
        .Select(row => MapToResponse(row, catalog))
        .ToList();

    return Results.Ok(response);
});

app.MapGet("/orders/{id:guid}", async (
    Guid id,
    OnlineStoreDbContext db,
    InMemoryOrderStore catalog,
    CancellationToken ct) =>
{
    OrderRow? row = await db.Orders
        .AsNoTracking()
        .Include(o => o.Lines)
        .FirstOrDefaultAsync(o => o.Id == id, ct);

    return row is null
        ? Results.NotFound(new { error = $"Order '{id}' was not found." })
        : Results.Ok(MapToResponse(row, catalog));
});

app.MapPost("/orders", async (
    CreateOrderRequest request,
    OnlineStoreDbContext db,
    InMemoryOrderStore catalog,
    CancellationToken ct) =>
{
    if (request.Items is null || request.Items.Count == 0)
    {
        return Results.BadRequest(new { error = "Order must contain at least one item." });
    }

    Customer? customer = catalog.GetCustomerById(request.CustomerId);
    if (customer is null)
    {
        return Results.BadRequest(new { error = $"Customer '{request.CustomerId}' was not found." });
    }

    var order = new Order(customer);
    try
    {
        foreach (CreateOrderItemRequest item in request.Items)
        {
            Product? product = catalog.GetProductById(item.ProductId);
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

    var row = new OrderRow
    {
        Id = order.Id,
        CustomerId = order.Customer.Id,
        Status = order.Status.ToString(),
        Lines = order.Items.Select(i => new OrderLineRow
        {
            Id = Guid.NewGuid(),
            OrderRowId = order.Id,
            ProductName = i.Product.Name,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList()
    };

    db.Orders.Add(row);
    await db.SaveChangesAsync(ct);

    return Results.Created($"/orders/{row.Id}", MapToResponse(row, catalog));
});

app.MapPost("/orders/{id:guid}/pay", async (
    Guid id,
    OnlineStoreDbContext db,
    InMemoryOrderStore catalog,
    OrderApplicationService orderService,
    CancellationToken ct) =>
{
    OrderRow? row = await db.Orders
        .Include(o => o.Lines)
        .FirstOrDefaultAsync(o => o.Id == id, ct);

    if (row is null)
    {
        return Results.NotFound(new { error = $"Order '{id}' was not found." });
    }

    Customer customer = catalog.GetCustomerById(row.CustomerId) ?? new Customer
    {
        Id = row.CustomerId,
        Name = "Unknown Customer",
        Email = "unknown@example.com",
        Address = new Address("Unknown", "Unknown", "1", "00000")
    };

    var domainOrder = new Order(customer, row.Id);
    foreach (OrderLineRow line in row.Lines)
    {
        domainOrder.AddItem(new Product(line.ProductName, line.UnitPrice), line.Quantity);
    }

    if (Enum.TryParse<OrderStatus>(row.Status, out OrderStatus persistedStatus))
    {
        if (persistedStatus == OrderStatus.Paid)
        {
            domainOrder.Pay(new CardPayment("4242"));
        }
        else if (persistedStatus == OrderStatus.Cancelled)
        {
            domainOrder.Cancel();
        }
    }

    try
    {
        orderService.ProcessPayment(domainOrder, new CardPayment("4242"));
        row.Status = domainOrder.Status.ToString();
        await db.SaveChangesAsync(ct);
        return Results.Ok(MapToResponse(row, catalog));
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

app.Run();

static OrderResponse MapToResponse(OrderRow row, InMemoryOrderStore catalog)
{
    string customerName = catalog.GetCustomerById(row.CustomerId)?.Name ?? "Unknown Customer";
    IReadOnlyList<OrderItemResponse> items = row.Lines
        .Select(l => new OrderItemResponse(
            l.Id,
            l.ProductName,
            l.UnitPrice,
            l.Quantity,
            l.UnitPrice * l.Quantity))
        .ToList();

    decimal total = items.Sum(i => i.Subtotal);
    return new OrderResponse(row.Id, row.CustomerId, customerName, row.Status, total, items);
}
