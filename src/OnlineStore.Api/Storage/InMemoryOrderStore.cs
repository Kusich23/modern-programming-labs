using System.Collections.Concurrent;
using OnlineStore.Domain;

namespace OnlineStore.Api.Storage;

/// <summary>
/// Thread-safe in-memory storage for customers, products, and orders.
/// </summary>
public sealed class InMemoryOrderStore
{
    public static readonly Guid DefaultCustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid LaptopProductId = Guid.Parse("22222222-2222-2222-2222-222222222221");
    public static readonly Guid MouseProductId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid KeyboardProductId = Guid.Parse("22222222-2222-2222-2222-222222222223");

    public static readonly Guid CreatedOrderId = Guid.Parse("33333333-3333-3333-3333-333333333331");
    public static readonly Guid PaidOrderId = Guid.Parse("33333333-3333-3333-3333-333333333332");
    public static readonly Guid CancelledOrderId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly ConcurrentDictionary<Guid, Customer> _customers = new();
    private readonly ConcurrentDictionary<Guid, Product> _products = new();
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public InMemoryOrderStore()
    {
        var customer = new Customer
        {
            Id = DefaultCustomerId,
            Name = "Кущов Максим Андрійович",
            Email = "maksym.kushchov@example.com",
            Address = new Address("Kyiv", "Khreshchatyk St", "22", "01001"),
            PhoneNumber = "+380501234567"
        };
        _customers.TryAdd(customer.Id, customer);

        var laptop = new Product("Laptop Pro 15", 35000m, LaptopProductId);
        var mouse = new Product("Wireless Mouse", 1200m, MouseProductId);
        var keyboard = new Product("Mechanical Keyboard", 3200m, KeyboardProductId);

        _products.TryAdd(laptop.Id, laptop);
        _products.TryAdd(mouse.Id, mouse);
        _products.TryAdd(keyboard.Id, keyboard);

        var createdOrder = new Order(customer, CreatedOrderId);
        createdOrder.AddItem(laptop, 1);
        createdOrder.AddItem(mouse, 1);
        _orders.TryAdd(createdOrder.Id, createdOrder);

        var paidOrder = new Order(customer, PaidOrderId);
        paidOrder.AddItem(keyboard, 1);
        paidOrder.Pay(new CardPayment("4242"));
        _orders.TryAdd(paidOrder.Id, paidOrder);

        var cancelledOrder = new Order(customer, CancelledOrderId);
        cancelledOrder.AddItem(mouse, 2);
        cancelledOrder.Cancel();
        _orders.TryAdd(cancelledOrder.Id, cancelledOrder);
    }

    public IReadOnlyCollection<Order> GetOrders(OrderStatus? status = null)
    {
        IEnumerable<Order> query = _orders.Values;
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        return query.OrderBy(o => o.Id).ToList();
    }

    public Order? GetOrderById(Guid id) => _orders.GetValueOrDefault(id);

    public Customer? GetCustomerById(Guid id) => _customers.GetValueOrDefault(id);

    public Product? GetProductById(Guid id) => _products.GetValueOrDefault(id);

    public bool TryAddOrder(Order order) => _orders.TryAdd(order.Id, order);
}
