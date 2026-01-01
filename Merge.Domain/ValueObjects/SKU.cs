namespace Merge.Domain.ValueObjects;

public record SKU
{
    public string Value { get; }

    public SKU(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SKU cannot be empty", nameof(value));

        var normalized = value.Trim().ToUpperInvariant();

        if (!IsValidSKU(normalized))
            throw new ArgumentException("Invalid SKU format. SKU must be 3-50 alphanumeric characters, optionally separated by hyphens or underscores", nameof(value));

        Value = normalized;
    }

    private static bool IsValidSKU(string sku) =>
        System.Text.RegularExpressions.Regex.IsMatch(sku, @"^[A-Z0-9][A-Z0-9\-_]{1,48}[A-Z0-9]$|^[A-Z0-9]{3}$");

    public static SKU Generate(string prefix, int id) =>
        new($"{prefix.ToUpperInvariant()}-{id:D6}");

    public static SKU Generate(string category, string brand, int sequence) =>
        new($"{category.ToUpperInvariant()[..Math.Min(3, category.Length)]}-{brand.ToUpperInvariant()[..Math.Min(3, brand.Length)]}-{sequence:D6}");

    public bool StartsWith(string prefix) => Value.StartsWith(prefix.ToUpperInvariant());

    public bool Contains(string segment) => Value.Contains(segment.ToUpperInvariant());

    public static implicit operator string(SKU sku) => sku.Value;

    public override string ToString() => Value;
}
