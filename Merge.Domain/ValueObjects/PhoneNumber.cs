namespace Merge.Domain.ValueObjects;

public record PhoneNumber
{
    public string Value { get; }
    public string CountryCode { get; }
    public string Number { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty", nameof(value));

        var cleaned = CleanPhoneNumber(value);

        if (!IsValidPhoneNumber(cleaned))
            throw new ArgumentException("Invalid phone number format", nameof(value));

        Value = cleaned;
        (CountryCode, Number) = ParsePhoneNumber(cleaned);
    }

    public PhoneNumber(string countryCode, string number)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code cannot be empty", nameof(countryCode));
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Number cannot be empty", nameof(number));

        var cleanedCountryCode = countryCode.TrimStart('+');
        var cleanedNumber = new string(number.Where(char.IsDigit).ToArray());

        if (!IsValidCountryCode(cleanedCountryCode))
            throw new ArgumentException("Invalid country code", nameof(countryCode));
        if (!IsValidNumber(cleanedNumber))
            throw new ArgumentException("Invalid phone number", nameof(number));

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
