namespace Merge.Domain.ValueObjects;

public record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));
        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format", nameof(value));
        Value = value.ToLowerInvariant();
    }

    private static bool IsValidEmail(string email) =>
        System.Text.RegularExpressions.Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public static implicit operator string(Email email) => email.Value;
}
