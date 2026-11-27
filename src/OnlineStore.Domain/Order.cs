namespace OnlineStore.Domain;

/// <summary>
/// Represents a customer order with encapsulated line items and lifecycle state transitions.
/// </summary>
public class Order
{
    private readonly List<OrderItem> _items = [];

    public event EventHandler<Order>? OrderPaid;

    public Guid Id { get; }
    public Customer Customer { get; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items;
    public decimal Total => _items.Sum(item => item.Subtotal);

    public Order(Customer customer, Guid? id = null)
    {
        ArgumentNullException.ThrowIfNull(customer);

        Id = id ?? Guid.NewGuid();
        Customer = customer;
        Status = OrderStatus.Created;
    }

    /// <summary>
    /// Adds a product with the specified quantity to the order while it is in Created status.
    /// </summary>
    public void AddItem(Product product, int quantity, Action<string>? logger = null)
    {
        EnsureModifiable();
        ArgumentNullException.ThrowIfNull(product);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        OrderItem? existingItem = _items.FirstOrDefault(item => item.Product.Id == product.Id);
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            logger?.Invoke($"Order {Id}: increased '{product.Name}' quantity by {quantity} (new total: {Total:F2} UAH).");
            return;
        }

        _items.Add(new OrderItem(product, quantity));
        logger?.Invoke($"Order {Id}: added {quantity}x '{product.Name}' (new total: {Total:F2} UAH).");
    }

    /// <summary>
    /// Removes a product line item from the order while it is in Created status.
    /// </summary>
    public bool RemoveItem(Guid productId, Action<string>? logger = null)
    {
        EnsureModifiable();

        OrderItem? item = _items.FirstOrDefault(i => i.Product.Id == productId);
        if (item is null)
        {
            return false;
        }

        bool removed = _items.Remove(item);
        if (removed)
        {
            logger?.Invoke($"Order {Id}: removed '{item.Product.Name}' (new total: {Total:F2} UAH).");
        }

        return removed;
    }

    /// <summary>
    /// Pays for the order using the provided payment method and optional discount policy, then transitions status to Paid.
    /// </summary>
    public void Pay(
        IPaymentMethod paymentMethod,
        Action<string>? logger = null,
        Func<decimal, decimal>? discountPolicy = null)
    {
        ArgumentNullException.ThrowIfNull(paymentMethod);

        if (Status == OrderStatus.Paid)
        {
            throw new InvalidOperationException("Order has already been paid.");
        }

        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot pay for a cancelled order.");
        }

        if (Status != OrderStatus.Created)
        {
            throw new InvalidOperationException($"Cannot pay for an order in '{Status}' status.");
        }

        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Cannot pay for an empty order.");
        }

        decimal amountToPay = discountPolicy is not null ? discountPolicy(Total) : Total;
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amountToPay);

        paymentMethod.Pay(amountToPay);
        Status = OrderStatus.Paid;
        logger?.Invoke($"Order {Id}: paid {amountToPay:F2} UAH via {paymentMethod.Name}.");
        OrderPaid?.Invoke(this, this);
    }

    /// <summary>
    /// Cancels the order if it has not been paid or already cancelled.
    /// </summary>
    public void Cancel(Action<string>? logger = null)
    {
        if (Status == OrderStatus.Paid)
        {
            throw new InvalidOperationException("Cannot cancel an order that has already been paid.");
        }

        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Order is already cancelled.");
        }

        if (Status != OrderStatus.Created)
        {
            throw new InvalidOperationException($"Cannot cancel an order in '{Status}' status.");
        }

        Status = OrderStatus.Cancelled;
        logger?.Invoke($"Order {Id}: order cancelled.");
    }

    /// <summary>
    /// Returns a human-readable description of the current order status.
    /// </summary>
    public string GetStatusDescription() => Status switch
    {
        OrderStatus.Created => "Order has been created and is awaiting payment.",
        OrderStatus.Paid => "Order has been paid and is being prepared for shipment.",
        OrderStatus.Shipped => "Order has been shipped to the delivery address.",
        OrderStatus.Delivered => "Order has been delivered to the customer.",
        OrderStatus.Cancelled => "Order has been cancelled.",
        _ => "Unknown order status."
    };

    /// <summary>
    /// Categorizes the order size based on its total amount.
    /// </summary>
    public string GetOrderCategory() => Total switch
    {
        < 500m => "Small",
        < 5000m => "Medium",
        _ => "Large"
    };

    /// <summary>
    /// Determines the delivery type using property pattern matching on status and total.
    /// </summary>
    public string GetDeliveryType() => this switch
    {
        { Status: OrderStatus.Paid, Total: >= 2000m } => "Free Delivery",
        { Status: OrderStatus.Cancelled } => "No Delivery (Cancelled)",
        _ => "Standard Delivery"
    };

    private void EnsureModifiable()
    {
        if (Status != OrderStatus.Created)
        {
            throw new InvalidOperationException($"Cannot modify items of an order in '{Status}' status.");
        }
    }
}
