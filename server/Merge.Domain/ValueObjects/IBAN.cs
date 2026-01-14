using Merge.Domain.Exceptions;

namespace Merge.Domain.ValueObjects;

/// <summary>
/// IBAN Value Object - BOLUM 1.3: Value Objects (ZORUNLU)
/// </summary>
public record IBAN
{
    public string Value { get; }

    public IBAN(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("IBAN boş olamaz");

        var cleaned = value.Replace(" ", "").Replace("-", "").ToUpperInvariant();

        if (cleaned.Length < 15 || cleaned.Length > 34)
            throw new DomainException("IBAN uzunluğu geçersiz (15-34 karakter arası olmalıdır)");

        if (!cleaned.All(c => char.IsLetterOrDigit(c)))
            throw new DomainException("IBAN sadece harf ve rakam içerebilir");

        if (!IsValidIBAN(cleaned))
            throw new DomainException("Geçersiz IBAN formatı");

        Value = cleaned;
    }

    private static bool IsValidIBAN(string iban)
    {
        if (iban.Length < 4)
            return false;

        // Move first 4 characters to end
        var rearranged = iban[4..] + iban[..4];

        // Replace letters with numbers (A=10, B=11, ..., Z=35)
        var numericString = string.Concat(rearranged.Select(c =>
            char.IsLetter(c) ? (c - 'A' + 10).ToString() : c.ToString()));

        // Calculate mod 97
        if (decimal.TryParse(numericString, out var numeric))
        {
            return numeric % 97 == 1;
        }

        return false;
    }

    public string ToFormattedString()
    {
        // Format as groups of 4 characters
        var formatted = new System.Text.StringBuilder();
        for (int i = 0; i < Value.Length; i += 4)
        {
            if (i > 0)
                formatted.Append(' ');
            formatted.Append(Value.Substring(i, Math.Min(4, Value.Length - i)));
        }
        return formatted.ToString();
    }

    public static implicit operator string(IBAN iban) => iban.Value;
}
