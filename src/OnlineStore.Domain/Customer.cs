namespace OnlineStore.Domain;

/// <summary>
/// Represents a registered customer of the online store.
/// </summary>
public class Customer
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required Address Address { get; init; }
    public string? PhoneNumber { get; init; }
}
