namespace OnlineStore.Api.Persistence;

/// <summary>
/// EF Core persistence entity representing a line item inside a <see cref="ReturnRequestRow"/>.
/// </summary>
public sealed class ReturnItemRow
{
    public Guid Id { get; set; }
    public Guid ReturnRequestRowId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public ReturnRequestRow? ReturnRequestRow { get; set; }
}
