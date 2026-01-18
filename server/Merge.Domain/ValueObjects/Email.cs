using Merge.Domain.Exceptions;

namespace Merge.Domain.ValueObjects;


public record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email boş olamaz");
        if (!IsValidEmail(value))
            throw new DomainException("Geçersiz email formatı");
        Value = value.ToLowerInvariant().Trim();
    }

    private static bool IsValidEmail(string email) =>
        System.Text.RegularExpressions.Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public static implicit operator string(Email email) => email.Value;
}
