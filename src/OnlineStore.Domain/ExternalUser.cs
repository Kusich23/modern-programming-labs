namespace OnlineStore.Domain;

/// <summary>
/// Read-only DTO representing an external contact retrieved from a remote directory API.
/// </summary>
public sealed record ExternalUser(int Id, string Name, string Email);
