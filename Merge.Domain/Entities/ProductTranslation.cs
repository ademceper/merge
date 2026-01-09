using Merge.Domain.Common;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// ProductTranslation Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ProductTranslation : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid ProductId { get; private set; }
    public Guid LanguageId { get; private set; }
    public string LanguageCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ShortDescription { get; private set; } = string.Empty;
    public string MetaTitle { get; private set; } = string.Empty;
    public string MetaDescription { get; private set; } = string.Empty;
    public string MetaKeywords { get; private set; } = string.Empty;

    // Navigation properties
    public Product Product { get; private set; } = null!;
    public Language Language { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ProductTranslation() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static ProductTranslation Create(
        Guid productId,
        Guid languageId,
        string languageCode,
        string name,
        string description = "",
        string shortDescription = "",
        string metaTitle = "",
        string metaDescription = "",
        string metaKeywords = "")
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstDefault(languageId, nameof(languageId));
        Guard.AgainstNullOrEmpty(languageCode, nameof(languageCode));
        Guard.AgainstNullOrEmpty(name, nameof(name));

        return new ProductTranslation
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            LanguageId = languageId,
            LanguageCode = languageCode.ToLowerInvariant(),
            Name = name,
            Description = description,
            ShortDescription = shortDescription,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Method - Update translation
    public void Update(
        string name,
        string description = "",
        string shortDescription = "",
        string metaTitle = "",
        string metaDescription = "",
        string metaKeywords = "")
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));

        Name = name;
        Description = description;
        ShortDescription = shortDescription;
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

