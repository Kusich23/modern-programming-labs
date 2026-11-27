namespace OnlineStore.Domain;

/// <summary>
/// Processes payments via bank wire transfer.
/// </summary>
public class BankTransferPayment : IPaymentMethod
{
    public string Iban { get; }
    public string Name => "Bank Transfer";
    public string? LastTransactionDetails { get; private set; }

    public BankTransferPayment(string iban = "UA213223130000026007233566001")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(iban);
        Iban = iban.Trim();
    }

    /// <inheritdoc />
    public void Pay(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        LastTransactionDetails = $"Transferred {amount:F2} UAH to IBAN {Iban}";
    }
}
