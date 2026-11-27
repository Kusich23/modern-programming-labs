namespace OnlineStore.Domain;

/// <summary>
/// Handles order payment events by recording audit log entries.
/// </summary>
public class LoggingService
{
    private readonly Action<string> _output;

    public LoggingService(Action<string> output)
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
        _output($"[AuditLog] Order {order.Id} paid by customer {order.Customer.Name} ({order.Items.Count} items).");
    }
}
