namespace OnlineStore.Domain;

/// <summary>
/// In-memory notification service storing sent messages for verification and testing.
/// </summary>
public sealed class InMemoryNotificationService : INotificationService
{
    private readonly List<string> _messages = [];

    public IReadOnlyList<string> Messages => _messages;

    /// <inheritdoc />
    public void Notify(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _messages.Add(message);
    }
}
