namespace OnlineStore.Api.Persistence;

/// <summary>
/// EF Core persistence entity representing a return request in SQLite.
/// </summary>
public sealed class ReturnRequestRow
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<ReturnItemRow> Items { get; set; } = [];
}
