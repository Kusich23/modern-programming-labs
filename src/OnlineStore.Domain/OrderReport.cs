namespace OnlineStore.Domain;

/// <summary>
/// Read-only DTO representing an exported order summary for reporting and JSON serialization.
/// </summary>
public sealed record OrderReport(
    Guid OrderId,
    string CustomerName,
    decimal Total,
    string Status,
    int ItemsCount)
{
    /// <summary>
    /// Creates an <see cref="OrderReport"/> snapshot from a domain <see cref="Order"/> without weakening encapsulation.
    /// </summary>
    public static OrderReport FromOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new OrderReport(
            OrderId: order.Id,
            CustomerName: order.Customer.Name,
            Total: order.Total,
            Status: order.Status.ToString(),
            ItemsCount: order.Items.Count);
    }
}
