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

app.MapGet("/returns", async (
    ReturnStatus? status,
    OnlineStoreDbContext db,
    CancellationToken ct) =>
{
    IQueryable<ReturnRequestRow> query = db.ReturnRequests
        .AsNoTracking()
        .Include(r => r.Items);

    if (status.HasValue)
    {
        string statusText = status.Value.ToString();
        query = query.Where(r => r.Status == statusText);
    }

    List<ReturnRequestRow> rows = await query
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync(ct);

    return Results.Ok(rows.Select(ReturnRequestResponse.FromRow).ToList());
});

app.MapGet("/returns/{id:guid}", async (
    Guid id,
    OnlineStoreDbContext db,
    CancellationToken ct) =>
{
    ReturnRequestRow? row = await db.ReturnRequests
        .AsNoTracking()
        .Include(r => r.Items)
        .FirstOrDefaultAsync(r => r.Id == id, ct);

    return row is null
        ? Results.NotFound(new { error = $"Return request '{id}' was not found." })
        : Results.Ok(ReturnRequestResponse.FromRow(row));
});

app.MapPost("/returns", async (
    CreateReturnRequest request,
    OnlineStoreDbContext db,
    CancellationToken ct) =>
{
    if (request.Items is null || request.Items.Count == 0)
    {
        return Results.BadRequest(new { error = "Return request must contain at least one item." });
    }

    OrderRow? orderRow = await db.Orders
        .AsNoTracking()
        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

    if (orderRow is null)
    {
        return Results.NotFound(new { error = $"Order '{request.OrderId}' was not found." });
    }

    if (orderRow.Status != OrderStatus.Paid.ToString())
    {
        return Results.Conflict(new { error = $"Return request can only be created for Paid orders (current status: '{orderRow.Status}')." });
    }

    ReturnRequest domainReturn;
    try
    {
        domainReturn = new ReturnRequest(request.OrderId, request.Reason);
        foreach (CreateReturnItemRequest item in request.Items)
        {
            domainReturn.AddItem(item.ProductName, item.Quantity, item.UnitPrice);
        }
    }
    catch (Exception ex) when (ex is ArgumentException or DomainRuleViolationException)
    {
        return Results.BadRequest(new { error = ex.Message });
    }

    var row = new ReturnRequestRow
    {
        Id = domainReturn.Id,
        OrderId = domainReturn.OrderId,
        Reason = domainReturn.Reason,
        Status = domainReturn.Status.ToString(),
        CreatedAt = domainReturn.CreatedAt,
        Items = domainReturn.Items.Select(i => new ReturnItemRow
        {
            Id = i.Id,
            ReturnRequestRowId = domainReturn.Id,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList()
    };

    db.ReturnRequests.Add(row);
    await db.SaveChangesAsync(ct);

    return Results.Created($"/returns/{row.Id}", ReturnRequestResponse.FromRow(row));
});

app.MapPost("/returns/{id:guid}/approve", async (
    Guid id,
    OnlineStoreDbContext db,
    CancellationToken ct) =>
{
    ReturnRequestRow? row = await db.ReturnRequests
        .Include(r => r.Items)
        .FirstOrDefaultAsync(r => r.Id == id, ct);

    if (row is null)
    {
        return Results.NotFound(new { error = $"Return request '{id}' was not found." });
    }

    ReturnRequest domainReturn = ReconstructReturnRequest(row);
    try
    {
        domainReturn.Approve();
        row.Status = domainReturn.Status.ToString();
        await db.SaveChangesAsync(ct);
        return Results.Ok(ReturnRequestResponse.FromRow(row));
    }
    catch (Exception ex) when (ex is InvalidOperationException or DomainRuleViolationException)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

app.MapPost("/returns/{id:guid}/reject", async (
    Guid id,
    OnlineStoreDbContext db,
    CancellationToken ct) =>
{
    ReturnRequestRow? row = await db.ReturnRequests
        .Include(r => r.Items)
        .FirstOrDefaultAsync(r => r.Id == id, ct);

    if (row is null)
    {
        return Results.NotFound(new { error = $"Return request '{id}' was not found." });
    }

    ReturnRequest domainReturn = ReconstructReturnRequest(row);
    try
    {
        domainReturn.Reject();
        row.Status = domainReturn.Status.ToString();
        await db.SaveChangesAsync(ct);
        return Results.Ok(ReturnRequestResponse.FromRow(row));
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

app.Run();

static ReturnRequest ReconstructReturnRequest(ReturnRequestRow row)
{
    var domainReturn = new ReturnRequest(row.OrderId, row.Reason, row.Id, row.CreatedAt);
    foreach (ReturnItemRow item in row.Items)
    {
        domainReturn.AddItem(item.ProductName, item.Quantity, item.UnitPrice);
    }

    if (Enum.TryParse<ReturnStatus>(row.Status, out ReturnStatus persistedStatus))
    {
        if (persistedStatus == ReturnStatus.Approved)
        {
            domainReturn.Approve();
        }
        else if (persistedStatus == ReturnStatus.Rejected)
        {
            domainReturn.Reject();
        }
    }

    return domainReturn;
}

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
