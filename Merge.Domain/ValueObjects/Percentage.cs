namespace Merge.Domain.ValueObjects;

public record Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(value), "Percentage must be between 0 and 100");

        Value = Math.Round(value, 2);
    }

    public static Percentage Zero => new(0);
    public static Percentage Full => new(100);
    public static Percentage Half => new(50);

    public static Percentage FromDecimal(decimal decimalValue)
    {
        if (decimalValue < 0 || decimalValue > 1)
            throw new ArgumentOutOfRangeException(nameof(decimalValue), "Decimal value must be between 0 and 1");
        return new Percentage(decimalValue * 100);
    }

    public decimal ToDecimal() => Value / 100;

    public decimal ApplyTo(decimal amount) => amount * ToDecimal();

    public decimal CalculateValueFrom(decimal total) => total * ToDecimal();

    public Percentage Add(Percentage other)
    {
        var result = Value + other.Value;
        return result > 100
            ? throw new InvalidOperationException("Resulting percentage cannot exceed 100")
            : new Percentage(result);
    }

    public Percentage Subtract(Percentage other)
    {
        var result = Value - other.Value;
        return result < 0
            ? throw new InvalidOperationException("Resulting percentage cannot be negative")
            : new Percentage(result);
    }

    public Percentage Complement() => new(100 - Value);

    public static implicit operator decimal(Percentage percentage) => percentage.Value;

    public override string ToString() => $"{Value:F2}%";
}
