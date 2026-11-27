namespace OnlineStore.Domain;

/// <summary>
/// Represents a registered customer of the online store.
/// </summary>
public class Customer
{
    public Guid Id { get; }
    public string Name { get; }
    public string Email { get; }

    public Customer(string name, string email, Guid? id = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        if (!email.Contains('@'))
        {
            throw new ArgumentException("Customer email must be a valid email address.", nameof(email));
        }

        Id = id ?? Guid.NewGuid();
        Name = name.Trim();
        Email = email.Trim();
    }
}
