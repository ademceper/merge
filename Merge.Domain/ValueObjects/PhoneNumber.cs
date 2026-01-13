using Merge.Domain.Exceptions;

namespace Merge.Domain.ValueObjects;

/// <summary>
/// PhoneNumber Value Object - BOLUM 1.3: Value Objects (ZORUNLU)
/// </summary>
public record PhoneNumber
{
    public string Value { get; }
    public string CountryCode { get; }
    public string Number { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Telefon numarası boş olamaz");

        var cleaned = CleanPhoneNumber(value);

        if (!IsValidPhoneNumber(cleaned))
            throw new DomainException("Geçersiz telefon numarası formatı");

        Value = cleaned;
        (CountryCode, Number) = ParsePhoneNumber(cleaned);
    }

    public PhoneNumber(string countryCode, string number)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new DomainException("Ülke kodu boş olamaz");
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Telefon numarası boş olamaz");

        var cleanedCountryCode = countryCode.TrimStart('+');
        var cleanedNumber = new string(number.Where(char.IsDigit).ToArray());

        if (!IsValidCountryCode(cleanedCountryCode))
            throw new DomainException("Geçersiz ülke kodu");
        if (!IsValidNumber(cleanedNumber))
            throw new DomainException("Geçersiz telefon numarası");

        CountryCode = cleanedCountryCode;
        Number = cleanedNumber;
        Value = $"+{CountryCode}{Number}";
    }

    private static string CleanPhoneNumber(string phone)
    {
        var cleaned = phone.Trim();
        if (!cleaned.StartsWith('+'))
            cleaned = $"+{cleaned}";
        return new string(cleaned.Where(c => char.IsDigit(c) || c == '+').ToArray());
    }

    private static bool IsValidPhoneNumber(string phone) =>
        System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+[1-9]\d{6,14}$");

    private static bool IsValidCountryCode(string countryCode) =>
        System.Text.RegularExpressions.Regex.IsMatch(countryCode, @"^[1-9]\d{0,3}$");

    private static bool IsValidNumber(string number) =>
        System.Text.RegularExpressions.Regex.IsMatch(number, @"^\d{4,14}$");

    private static (string CountryCode, string Number) ParsePhoneNumber(string phone)
    {
        var withoutPlus = phone.TrimStart('+');

        // Try common country code lengths (1-4 digits)
        // Default to first 2 digits as country code for Turkish numbers
        if (withoutPlus.StartsWith("90") && withoutPlus.Length >= 12)
            return ("90", withoutPlus[2..]);
        if (withoutPlus.StartsWith("1") && withoutPlus.Length >= 11)
            return ("1", withoutPlus[1..]);

        // Default: assume 2-digit country code
        return withoutPlus.Length > 2
            ? (withoutPlus[..2], withoutPlus[2..])
            : (withoutPlus, string.Empty);
    }

    public string ToFormattedString() => $"+{CountryCode} {Number}";

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
