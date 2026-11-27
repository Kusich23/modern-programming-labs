namespace OnlineStore.Domain;

/// <summary>
/// Represents an immutable postal delivery address value object.
/// </summary>
public record Address(string City, string Street, string Building, string PostalCode);
