namespace OnlineStore.Domain;

/// <summary>
/// Represents a single line item within an order.
/// </summary>
public class OrderItem
{
    public Product Product { get; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; }
    public decimal Subtotal => UnitPrice * Quantity;

    public OrderItem(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        Product = product;
        Quantity = quantity;
        UnitPrice = product.Price;
    }

    /// <summary>
    /// Increases the item quantity by the specified positive amount.
    /// </summary>
    public void IncreaseQuantity(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        Quantity += amount;
    }
}
