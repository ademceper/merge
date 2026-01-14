using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;

namespace Merge.Domain.ValueObjects;

/// <summary>
/// Slug Value Object - BOLUM 1.3: Value Objects (ZORUNLU)
/// URL-friendly string representation for entities (Category, Product, Blog, etc.)
/// </summary>
public record Slug
{
    public string Value { get; }

    public Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Slug cannot be empty", nameof(value));

        var normalized = NormalizeSlug(value);

        if (!IsValidSlug(normalized))
            throw new ArgumentException("Invalid slug format", nameof(value));

        Value = normalized;
    }

    /// <summary>
    /// Normalize string to slug format
    /// </summary>
    private static string NormalizeSlug(string text)
    {
        var slug = text.Trim().ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace("--", "-")
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("İ", "i")
            .Replace("Ğ", "g")
            .Replace("Ü", "u")
            .Replace("Ş", "s")
            .Replace("Ö", "o")
            .Replace("Ç", "c");

        // Remove special characters except hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Remove multiple consecutive hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

        // Remove leading and trailing hyphens
        slug = slug.Trim('-');

        return slug;
    }

    /// <summary>
    /// Validate slug format: lowercase letters, numbers, and hyphens only
    /// </summary>
    private static bool IsValidSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        // Must start and end with alphanumeric character
        if (!char.IsLetterOrDigit(slug[0]) || !char.IsLetterOrDigit(slug[^1]))
            return false;

        // Must match pattern: lowercase letters, numbers, and hyphens
        return System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$");
    }

    /// <summary>
    /// Create slug from string (factory method)
    /// </summary>
    public static Slug FromString(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be empty", nameof(text));

        return new Slug(text);
    }

    /// <summary>
    /// Implicit conversion to string
    /// </summary>
    public static implicit operator string(Slug slug) => slug.Value;

    public override string ToString() => Value;
}

