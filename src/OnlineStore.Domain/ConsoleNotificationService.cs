namespace OnlineStore.Domain;

/// <summary>
/// Notification service that forwards messages to a configured output delegate.
/// </summary>
public sealed class ConsoleNotificationService(Action<string> output) : INotificationService
{
    private readonly Action<string> _output = output ?? throw new ArgumentNullException(nameof(output));

    /// <inheritdoc />
    public void Notify(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _output(message);
    }
}
