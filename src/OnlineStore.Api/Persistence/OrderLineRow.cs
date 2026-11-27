namespace OnlineStore.Api.Persistence;

/// <summary>
/// EF Core persistence entity representing a line item belonging to an <see cref="OrderRow"/>.
/// </summary>
public sealed class OrderLineRow
{
    public Guid Id { get; set; }
    public Guid OrderRowId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public OrderRow? OrderRow { get; set; }
}
