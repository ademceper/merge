using Merge.Domain.Exceptions;

namespace Merge.Domain.ValueObjects;

/// <summary>
/// URL Value Object - BOLUM 1.3: Value Objects (ZORUNLU)
/// </summary>
public record Url
{
    public string Value { get; }

    public Url(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("URL boş olamaz");

        var trimmed = value.Trim();

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            throw new DomainException("Geçersiz URL formatı");

        // Sadece HTTP ve HTTPS protokollerini kabul et
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            throw new DomainException("URL sadece HTTP veya HTTPS protokolünü destekler");

        if (trimmed.Length > 500)
            throw new DomainException("URL en fazla 500 karakter olabilir");

        Value = trimmed;
    }

    public static implicit operator string(Url url) => url.Value;
}
