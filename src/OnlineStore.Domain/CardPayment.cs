namespace OnlineStore.Domain;

/// <summary>
/// Processes payments using a bank card.
/// </summary>
public class CardPayment : IPaymentMethod
{
    public string CardLast4 { get; }
    public string Name => "Card Payment";
    public string? LastTransactionDetails { get; private set; }

    public CardPayment(string cardLast4 = "4242")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cardLast4);

        string trimmed = cardLast4.Trim();
        if (trimmed.Length != 4 || !trimmed.All(char.IsDigit))
        {
            throw new ArgumentException("Card last 4 digits must contain exactly 4 numeric characters.", nameof(cardLast4));
        }

        CardLast4 = trimmed;
    }

    /// <inheritdoc />
    public void Pay(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        LastTransactionDetails = $"Charged {amount:F2} UAH to card **** {CardLast4}";
    }
}
