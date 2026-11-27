namespace OnlineStore.Domain;

/// <summary>
/// Represents a customer return request linked to a paid order with encapsulated items and status rules.
/// </summary>
public class ReturnRequest
{
    private readonly List<ReturnItem> _items = [];

    public Guid Id { get; }
    public Guid OrderId { get; }
    public string Reason { get; }
    public ReturnStatus Status { get; private set; }
    public DateTime CreatedAt { get; }
    public IReadOnlyCollection<ReturnItem> Items => _items;
    public decimal TotalRefundAmount => _items.Sum(item => item.Subtotal);

    public ReturnRequest(Guid orderId, string reason, Guid? id = null, DateTime? createdAt = null)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order ID cannot be empty.", nameof(orderId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        Id = id ?? Guid.NewGuid();
        OrderId = orderId;
        Reason = reason.Trim();
        Status = ReturnStatus.Pending;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a return line item while the request is in Pending status.
    /// </summary>
    public void AddItem(string productName, int quantity, decimal unitPrice)
    {
        var item = new ReturnItem(productName, quantity, unitPrice);
        if (Status != ReturnStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot add items to a return request in '{Status}' status.");
        }

        _items.Add(item);
    }

    /// <summary>
    /// Approves the return request if it is Pending and contains at least one item.
    /// </summary>
    public void Approve()
    {
        if (Status != ReturnStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot approve a return request in '{Status}' status.");
        }

        if (_items.Count == 0)
        {
            throw new DomainRuleViolationException(
                "EMPTY_RETURN_REQUEST",
                "Cannot approve an empty return request without items.");
        }

        Status = ReturnStatus.Approved;
    }

    /// <summary>
    /// Rejects the return request if it is still in Pending status.
    /// </summary>
    public void Reject()
    {
        if (Status != ReturnStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot reject a return request in '{Status}' status.");
        }

        Status = ReturnStatus.Rejected;
    }
}
