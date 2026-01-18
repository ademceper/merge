using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.ValueObjects;


public record Address
{
    public string AddressLine1 { get; }
    public string? AddressLine2 { get; }
    public string City { get; }
    public string State { get; }
    public string Country { get; }
    public string PostalCode { get; }

    public Address(
        string addressLine1,
        string city,
        string country,
        string postalCode,
        string? addressLine2 = null,
        string? state = null)
    {
        if (string.IsNullOrWhiteSpace(addressLine1))
            throw new DomainException("Adres satırı 1 boş olamaz");
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("Şehir boş olamaz");
        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException("Ülke boş olamaz");
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new DomainException("Posta kodu boş olamaz");

        if (addressLine1.Length < 5 || addressLine1.Length > 500)
            throw new DomainException("Adres satırı 1 en az 5, en fazla 500 karakter olmalıdır");
        if (city.Length < 2 || city.Length > 100)
            throw new DomainException("Şehir en az 2, en fazla 100 karakter olmalıdır");
        if (country.Length < 2 || country.Length > 100)
            throw new DomainException("Ülke en az 2, en fazla 100 karakter olmalıdır");
        if (postalCode.Length < 3 || postalCode.Length > 20)
            throw new DomainException("Posta kodu en az 3, en fazla 20 karakter olmalıdır");

        AddressLine1 = addressLine1.Trim();
        AddressLine2 = addressLine2?.Trim();
        City = city.Trim();
        State = state?.Trim() ?? string.Empty;
        Country = country.Trim();
        PostalCode = postalCode.Trim();
    }

    public string ToFormattedString()
    {
        List<string> parts = [AddressLine1];
        if (!string.IsNullOrWhiteSpace(AddressLine2))
            parts.Add(AddressLine2);
        parts.Add($"{City}, {State}".TrimEnd(',', ' '));
        parts.Add($"{PostalCode} {Country}");
        return string.Join("\n", parts);
    }

    public static implicit operator string(Address address) => address.ToFormattedString();
}
