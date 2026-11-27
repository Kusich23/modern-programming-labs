namespace OnlineStore.Domain;

/// <summary>
/// Generic in-memory repository storing domain objects of type <typeparamref name="T"/>.
/// </summary>
public class InMemoryRepository<T> where T : class
{
    private readonly List<T> _items = [];

    /// <summary>
    /// Adds an item to the repository.
    /// </summary>
    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    /// <summary>
    /// Removes the specified item from the repository.
    /// </summary>
    public bool Remove(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _items.Remove(item);
    }

    /// <summary>
    /// Returns all items stored in the repository as a read-only collection.
    /// </summary>
    public IReadOnlyCollection<T> GetAll() => _items;
}
