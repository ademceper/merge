using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// SEOSettings Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class SEOSettings : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string PageType { get; private set; } = string.Empty; // Product, Category, Blog, Page, Home
    public Guid? EntityId { get; private set; } // ID of the entity (ProductId, CategoryId, etc.)
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public string? MetaKeywords { get; private set; }
    public string? CanonicalUrl { get; private set; }
    public string? OgTitle { get; private set; } // Open Graph title
    public string? OgDescription { get; private set; } // Open Graph description
    public string? OgImageUrl { get; private set; } // Open Graph image
    public string? TwitterCard { get; private set; } // summary, summary_large_image
    public string? StructuredData { get; private set; } // JSON-LD structured data
    public bool IsIndexed { get; private set; } = true; // Allow search engines to index
    public bool FollowLinks { get; private set; } = true; // Follow or nofollow
    public decimal Priority { get; private set; } = 0.5m; // Sitemap priority (0.0 to 1.0)
    public string? ChangeFrequency { get; private set; } // always, hourly, daily, weekly, monthly, yearly, never

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private SEOSettings() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static SEOSettings Create(
        string pageType,
        Guid? entityId = null,
        string? metaTitle = null,
        string? metaDescription = null,
        string? metaKeywords = null,
        string? canonicalUrl = null,
        string? ogTitle = null,
        string? ogDescription = null,
        string? ogImageUrl = null,
        string? twitterCard = null,
        string? structuredData = null,
        bool isIndexed = true,
        bool followLinks = true,
        decimal priority = 0.5m,
        string? changeFrequency = null)
    {
        Guard.AgainstNullOrEmpty(pageType, nameof(pageType));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MinSEOPriority=0.0, MaxSEOPriority=1.0, MaxPageTypeLength=50
        Guard.AgainstOutOfRange(priority, 0m, 1m, nameof(priority));
        Guard.AgainstLength(pageType, 50, nameof(pageType));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxMetaTitleLength=60, MaxMetaDescriptionLength=160, MaxMetaKeywordsLength=255, MaxOgTitleLength=60, MaxOgDescriptionLength=160, MaxTwitterCardLength=30
        if (metaTitle != null)
            Guard.AgainstLength(metaTitle, 60, nameof(metaTitle));
        if (metaDescription != null)
            Guard.AgainstLength(metaDescription, 160, nameof(metaDescription));
        if (metaKeywords != null)
            Guard.AgainstLength(metaKeywords, 255, nameof(metaKeywords));
        if (ogTitle != null)
            Guard.AgainstLength(ogTitle, 60, nameof(ogTitle));
        if (ogDescription != null)
            Guard.AgainstLength(ogDescription, 160, nameof(ogDescription));
        if (twitterCard != null)
            Guard.AgainstLength(twitterCard, 30, nameof(twitterCard));

        var validChangeFrequencies = new[] { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" };
        if (!string.IsNullOrEmpty(changeFrequency) && !validChangeFrequencies.Contains(changeFrequency.ToLowerInvariant()))
        {
            throw new DomainException($"Geçersiz change frequency. Geçerli değerler: {string.Join(", ", validChangeFrequencies)}");
        }

        // ✅ BOLUM 1.3: URL Validation - Domain layer'da URL validasyonu
        if (!string.IsNullOrEmpty(canonicalUrl) && !IsValidUrl(canonicalUrl))
        {
            throw new DomainException("Geçerli bir canonical URL giriniz.");
        }

        if (!string.IsNullOrEmpty(ogImageUrl) && !IsValidUrl(ogImageUrl))
        {
            throw new DomainException("Geçerli bir Open Graph image URL giriniz.");
        }

        var settings = new SEOSettings
        {
            Id = Guid.NewGuid(),
            PageType = pageType,
            EntityId = entityId,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords,
            CanonicalUrl = canonicalUrl,
            OgTitle = ogTitle,
            OgDescription = ogDescription,
            OgImageUrl = ogImageUrl,
            TwitterCard = twitterCard,
            StructuredData = structuredData,
            IsIndexed = isIndexed,
            FollowLinks = followLinks,
            Priority = priority,
            ChangeFrequency = changeFrequency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - SEOSettingsCreatedEvent yayınla (ÖNERİLİR)
        settings.AddDomainEvent(new SEOSettingsCreatedEvent(settings.Id, pageType, entityId));

        return settings;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update meta information
    public void UpdateMetaInformation(
        string? metaTitle,
        string? metaDescription,
        string? metaKeywords,
        string? canonicalUrl)
    {
        // ✅ BOLUM 1.3: URL Validation - Domain layer'da URL validasyonu
        if (!string.IsNullOrEmpty(canonicalUrl) && !IsValidUrl(canonicalUrl))
        {
            throw new DomainException("Geçerli bir canonical URL giriniz.");
        }
        
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxMetaTitleLength=60, MaxMetaDescriptionLength=160, MaxMetaKeywordsLength=255
        if (metaTitle != null)
            Guard.AgainstLength(metaTitle, 60, nameof(metaTitle));
        if (metaDescription != null)
            Guard.AgainstLength(metaDescription, 160, nameof(metaDescription));
        if (metaKeywords != null)
            Guard.AgainstLength(metaKeywords, 255, nameof(metaKeywords));
        
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
        CanonicalUrl = canonicalUrl;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SEOSettingsUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, PageType, EntityId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update Open Graph information
    public void UpdateOpenGraphInformation(
        string? ogTitle,
        string? ogDescription,
        string? ogImageUrl,
        string? twitterCard)
    {
        // ✅ BOLUM 1.3: URL Validation - Domain layer'da URL validasyonu
        if (!string.IsNullOrEmpty(ogImageUrl) && !IsValidUrl(ogImageUrl))
        {
            throw new DomainException("Geçerli bir Open Graph image URL giriniz.");
        }
        
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxOgTitleLength=60, MaxOgDescriptionLength=160, MaxTwitterCardLength=30
        if (ogTitle != null)
            Guard.AgainstLength(ogTitle, 60, nameof(ogTitle));
        if (ogDescription != null)
            Guard.AgainstLength(ogDescription, 160, nameof(ogDescription));
        if (twitterCard != null)
            Guard.AgainstLength(twitterCard, 30, nameof(twitterCard));
        
        OgTitle = ogTitle;
        OgDescription = ogDescription;
        OgImageUrl = ogImageUrl;
        TwitterCard = twitterCard;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SEOSettingsUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, PageType, EntityId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update structured data
    public void UpdateStructuredData(string? structuredData)
    {
        StructuredData = structuredData;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SEOSettingsUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, PageType, EntityId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update indexing settings
    public void UpdateIndexingSettings(bool isIndexed, bool followLinks)
    {
        IsIndexed = isIndexed;
        FollowLinks = followLinks;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SEOSettingsUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, PageType, EntityId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update page type
    public void UpdatePageType(string newPageType)
    {
        Guard.AgainstNullOrEmpty(newPageType, nameof(newPageType));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxPageTypeLength=50
        Guard.AgainstLength(newPageType, 50, nameof(newPageType));
        PageType = newPageType;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SEOSettingsUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, newPageType, EntityId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update entity ID
    public void UpdateEntityId(Guid? newEntityId)
    {
        EntityId = newEntityId;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SEOSettingsUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, PageType, newEntityId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update sitemap settings
    public void UpdateSitemapSettings(decimal priority, string? changeFrequency)
    {
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MinSEOPriority=0.0, MaxSEOPriority=1.0
        Guard.AgainstOutOfRange(priority, 0m, 1m, nameof(priority));

        var validChangeFrequencies = new[] { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" };
        if (!string.IsNullOrEmpty(changeFrequency) && !validChangeFrequencies.Contains(changeFrequency.ToLowerInvariant()))
        {
            throw new DomainException($"Geçersiz change frequency. Geçerli değerler: {string.Join(", ", validChangeFrequencies)}");
        }

        Priority = priority;
        ChangeFrequency = changeFrequency;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SEOSettingsUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, PageType, EntityId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SEOSettingsDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SEOSettingsDeletedEvent(Id, PageType, EntityId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Restore deleted SEO settings
    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SEOSettingsRestoredEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SEOSettingsRestoredEvent(Id, PageType, EntityId));
    }

    // ✅ BOLUM 1.3: URL Validation Helper Method
    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

