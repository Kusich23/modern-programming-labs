namespace OnlineStore.Domain;

/// <summary>
/// Represents a single product line item included in a return request.
/// </summary>
public class ReturnItem
{
    public Guid Id { get; }
    public string ProductName { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal Subtotal => UnitPrice * Quantity;

    public ReturnItem(string productName, int quantity, decimal unitPrice, Guid? id = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(unitPrice);

        Id = id ?? Guid.NewGuid();
        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
