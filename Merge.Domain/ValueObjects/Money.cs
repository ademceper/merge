namespace Merge.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "TRY")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter code", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = "TRY") => new(0, currency);
    public Money Add(Money other) => Currency == other.Currency
        ? new Money(Amount + other.Amount, Currency)
        : throw new InvalidOperationException("Cannot add different currencies");
    public Money Subtract(Money other) => Currency == other.Currency
        ? new Money(Amount - other.Amount, Currency)
        : throw new InvalidOperationException("Cannot subtract different currencies");
}
