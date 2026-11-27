namespace OnlineStore.Api.Persistence;

/// <summary>
/// EF Core persistence entity representing an order record in SQLite.
/// </summary>
public sealed class OrderRow
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderLineRow> Lines { get; set; } = [];
}
