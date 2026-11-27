namespace OnlineStore.Domain;

/// <summary>
/// Defines a notification abstraction for sending domain workflow messages.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification message.
    /// </summary>
    void Notify(string message);
}
