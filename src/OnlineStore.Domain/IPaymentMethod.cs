namespace OnlineStore.Domain;

/// <summary>
/// Defines a payment strategy contract for processing order payments.
/// </summary>
public interface IPaymentMethod
{
    string Name { get; }
    string? LastTransactionDetails { get; }

    /// <summary>
    /// Processes a payment for the specified positive amount.
    /// </summary>
    void Pay(decimal amount);
}
