using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Content;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductTranslation Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ProductTranslation : BaseEntity, IAggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid LanguageId { get; private set; }
    public string LanguageCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ShortDescription { get; private set; } = string.Empty;
    public string MetaTitle { get; private set; } = string.Empty;
    public string MetaDescription { get; private set; } = string.Empty;
    public string MetaKeywords { get; private set; } = string.Empty;

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Product Product { get; private set; } = null!;
    public Language Language { get; private set; } = null!;

    private ProductTranslation() { }

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
        Guard.AgainstLength(name, ValidationConstants.MaxNameLength, nameof(name));
        
        if (!string.IsNullOrEmpty(description))
        {
            Guard.AgainstLength(description, ValidationConstants.MaxDescriptionLength, nameof(description));
        }
        
        if (!string.IsNullOrEmpty(shortDescription))
        {
            Guard.AgainstLength(shortDescription, ValidationConstants.MaxShortDescriptionLength, nameof(shortDescription));
        }
        
        if (!string.IsNullOrEmpty(metaTitle))
        {
            Guard.AgainstLength(metaTitle, ValidationConstants.MaxMetaTitleLength, nameof(metaTitle));
        }
        
        if (!string.IsNullOrEmpty(metaDescription))
        {
            Guard.AgainstLength(metaDescription, ValidationConstants.MaxMetaDescriptionLength, nameof(metaDescription));
        }
        
        if (!string.IsNullOrEmpty(metaKeywords))
        {
            Guard.AgainstLength(metaKeywords, ValidationConstants.MaxMetaKeywordsLength, nameof(metaKeywords));
        }

        var translation = new ProductTranslation
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
        
        translation.ValidateInvariants();
        
        translation.AddDomainEvent(new ProductTranslationCreatedEvent(translation.Id, productId, languageId, languageCode, name));
        
        return translation;
    }

    public void Update(
        string name,
        string description = "",
        string shortDescription = "",
        string metaTitle = "",
        string metaDescription = "",
        string metaKeywords = "")
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, ValidationConstants.MaxNameLength, nameof(name));
        
        if (!string.IsNullOrEmpty(description))
        {
            Guard.AgainstLength(description, ValidationConstants.MaxDescriptionLength, nameof(description));
        }
        
        if (!string.IsNullOrEmpty(shortDescription))
        {
            Guard.AgainstLength(shortDescription, ValidationConstants.MaxShortDescriptionLength, nameof(shortDescription));
        }
        
        if (!string.IsNullOrEmpty(metaTitle))
        {
            Guard.AgainstLength(metaTitle, ValidationConstants.MaxMetaTitleLength, nameof(metaTitle));
        }
        
        if (!string.IsNullOrEmpty(metaDescription))
        {
            Guard.AgainstLength(metaDescription, ValidationConstants.MaxMetaDescriptionLength, nameof(metaDescription));
        }
        
        if (!string.IsNullOrEmpty(metaKeywords))
        {
            Guard.AgainstLength(metaKeywords, ValidationConstants.MaxMetaKeywordsLength, nameof(metaKeywords));
        }

        Name = name;
        Description = description;
        ShortDescription = shortDescription;
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new ProductTranslationUpdatedEvent(Id, ProductId, LanguageCode));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new ProductTranslationDeletedEvent(Id, ProductId, LanguageCode));
    }

    // Not: Application katmanındaki InternationalSettings ile senkronize tutulmalı
    private static class ValidationConstants
    {
        public const int MaxNameLength = 200;
        public const int MaxDescriptionLength = 5000;
        public const int MaxShortDescriptionLength = 500;
        public const int MaxMetaTitleLength = 200;
        public const int MaxMetaDescriptionLength = 500;
        public const int MaxMetaKeywordsLength = 200;
    }

    private void ValidateInvariants()
    {
        if (Guid.Empty == ProductId)
            throw new DomainException("Ürün ID boş olamaz");

        if (Guid.Empty == LanguageId)
            throw new DomainException("Dil ID boş olamaz");

        if (string.IsNullOrWhiteSpace(LanguageCode))
            throw new DomainException("Dil kodu boş olamaz");

        if (string.IsNullOrWhiteSpace(Name))
            throw new DomainException("Çeviri adı boş olamaz");

        Guard.AgainstLength(Name, ValidationConstants.MaxNameLength, nameof(Name));
        Guard.AgainstLength(Description, ValidationConstants.MaxDescriptionLength, nameof(Description));
        Guard.AgainstLength(ShortDescription, ValidationConstants.MaxShortDescriptionLength, nameof(ShortDescription));
        Guard.AgainstLength(MetaTitle, ValidationConstants.MaxMetaTitleLength, nameof(MetaTitle));
        Guard.AgainstLength(MetaDescription, ValidationConstants.MaxMetaDescriptionLength, nameof(MetaDescription));
        Guard.AgainstLength(MetaKeywords, ValidationConstants.MaxMetaKeywordsLength, nameof(MetaKeywords));
    }
}

