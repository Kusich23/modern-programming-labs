namespace OnlineStore.Domain;

/// <summary>
/// Provides extension methods for <see cref="Order"/>.
/// </summary>
public static class OrderExtensions
{
    /// <summary>
    /// Determines whether the order total meets or exceeds the large-order threshold (5000 UAH).
    /// </summary>
    public static bool IsLargeOrder(this Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return order.Total >= 5000m;
    }

    /// <summary>
    /// Returns a concise summary string containing order Id, status, item count, and total amount.
    /// </summary>
    public static string GetSummary(this Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return $"Order {order.Id} | Status: {order.Status} | Items: {order.Items.Count} | Total: {order.Total:F2} UAH";
    }
}
