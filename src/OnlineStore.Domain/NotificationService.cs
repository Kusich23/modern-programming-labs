namespace OnlineStore.Domain;

/// <summary>
/// Handles order payment events by dispatching customer notifications.
/// </summary>
public class NotificationService
{
    private readonly Action<string> _output;

    public NotificationService(Action<string> output)
    {
        ArgumentNullException.ThrowIfNull(output);
        _output = output;
    }

    /// <summary>
    /// Handles the <see cref="Order.OrderPaid"/> event.
    /// </summary>
    public void Handle(object? sender, Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        _output($"[Notification] Sent receipt to {order.Customer.Email} for order {order.Id} (Total: {order.Total:F2} UAH).");
    }
}
