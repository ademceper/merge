namespace Merge.Domain.Entities;

public class Language : BaseEntity
{
    public string Code { get; set; } = string.Empty; // en, tr, ar, de, fr
    public string Name { get; set; } = string.Empty; // English, Türkçe, العربية
    public string NativeName { get; set; } = string.Empty; // English, Türkçe, العربية
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsRTL { get; set; } = false; // Right-to-left (Arabic, Hebrew)
    public string FlagIcon { get; set; } = string.Empty; // URL or emoji flag
}

public class ProductTranslation : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string MetaKeywords { get; set; } = string.Empty;

    // Navigation properties
    public Product Product { get; set; } = null!;
    public Language Language { get; set; } = null!;
}

public class CategoryTranslation : BaseEntity
{
    public Guid CategoryId { get; set; }
    public Guid LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public Category Category { get; set; } = null!;
    public Language Language { get; set; } = null!;
}

public class UserLanguagePreference : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;

    // Navigation properties
    public User User { get; set; } = null!;
    public Language Language { get; set; } = null!;
}

public class StaticTranslation : BaseEntity
{
    public string Key { get; set; } = string.Empty; // e.g., "button.add_to_cart", "header.welcome"
    public Guid LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // UI, Email, Notification, etc.

    // Navigation properties
    public Language Language { get; set; } = null!;
}
