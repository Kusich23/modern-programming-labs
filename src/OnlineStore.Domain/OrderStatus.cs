namespace OnlineStore.Domain;

/// <summary>
/// Represents the lifecycle status of a customer order.
/// </summary>
public enum OrderStatus
{
    Created = 0,
    Paid = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}
