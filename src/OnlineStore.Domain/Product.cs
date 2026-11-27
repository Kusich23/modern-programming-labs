namespace OnlineStore.Domain;

/// <summary>
/// Represents a product available for purchase in the online store.
/// </summary>
public class Product
{
    public Guid Id { get; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    public Product(string name, decimal price, Guid? id = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        Id = id ?? Guid.NewGuid();
        Name = name.Trim();
        Price = price;
    }

    /// <summary>
    /// Updates the product price after validating that it remains positive.
    /// </summary>
    public void UpdatePrice(decimal newPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newPrice);
        Price = newPrice;
    }
}
