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

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private SEOSettings() { }

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
        // Configuration değerleri: MinSEOPriority=0.0, MaxSEOPriority=1.0, MaxPageTypeLength=50
        Guard.AgainstOutOfRange(priority, 0m, 1m, nameof(priority));
        Guard.AgainstLength(pageType, 50, nameof(pageType));
        // Configuration değerleri: MaxMetaTitleLength=60, MaxMetaDescriptionLength=160, MaxMetaKeywordsLength=255, MaxOgTitleLength=60, MaxOgDescriptionLength=160, MaxTwitterCardLength=30
        if (metaTitle is not null)
            Guard.AgainstLength(metaTitle, 60, nameof(metaTitle));
        if (metaDescription is not null)
            Guard.AgainstLength(metaDescription, 160, nameof(metaDescription));
        if (metaKeywords is not null)
            Guard.AgainstLength(metaKeywords, 255, nameof(metaKeywords));
        if (ogTitle is not null)
            Guard.AgainstLength(ogTitle, 60, nameof(ogTitle));
        if (ogDescription is not null)
            Guard.AgainstLength(ogDescription, 160, nameof(ogDescription));
        if (twitterCard is not null)
            Guard.AgainstLength(twitterCard, 30, nameof(twitterCard));

        var validChangeFrequencies = new[] { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" };
        if (!string.IsNullOrEmpty(changeFrequency) && !validChangeFrequencies.Contains(changeFrequency.ToLowerInvariant()))
        {
            throw new DomainException($"Geçersiz change frequency. Geçerli değerler: {string.Join(", ", validChangeFrequencies)}");
        }

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

        settings.AddDomainEvent(new SEOSettingsCreatedEvent(settings.Id, pageType, entityId));

        return settings;
    }

    public void UpdateMetaInformation(
        string? metaTitle,
        string? metaDescription,
        string? metaKeywords,
        string? canonicalUrl)
    {
        if (!string.IsNullOrEmpty(canonicalUrl) && !IsValidUrl(canonicalUrl))
        {
            throw new DomainException("Geçerli bir canonical URL giriniz.");
        }
        
        // Configuration değerleri: MaxMetaTitleLength=60, MaxMetaDescriptionLength=160, MaxMetaKeywordsLength=255
        if (metaTitle is not null)
            Guard.AgainstLength(metaTitle, 60, nameof(metaTitle));
        if (metaDescription is not null)
            Guard.AgainstLength(metaDescription, 160, nameof(metaDescription));
        if (metaKeywords is not null)
            Guard.AgainstLength(metaKeywords, 255, nameof(metaKeywords));
        
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
        CanonicalUrl = canonicalUrl;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, PageType, EntityId));
    }

    public void UpdateOpenGraphInformation(
        string? ogTitle,
        string? ogDescription,
        string? ogImageUrl,
        string? twitterCard)
    {
        if (!string.IsNullOrEmpty(ogImageUrl) && !IsValidUrl(ogImageUrl))
        {
            throw new DomainException("Geçerli bir Open Graph image URL giriniz.");
        }
        
        // Configuration değerleri: MaxOgTitleLength=60, MaxOgDescriptionLength=160, MaxTwitterCardLength=30
        if (ogTitle is not null)
            Guard.AgainstLength(ogTitle, 60, nameof(ogTitle));
        if (ogDescription is not null)
            Guard.AgainstLength(ogDescription, 160, nameof(ogDescription));
        if (twitterCard is not null)
            Guard.AgainstLength(twitterCard, 30, nameof(twitterCard));
        
        OgTitle = ogTitle;
        OgDescription = ogDescription;
        OgImageUrl = ogImageUrl;
        TwitterCard = twitterCard;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, PageType, EntityId));
    }

    public void UpdateStructuredData(string? structuredData)
    {
        StructuredData = structuredData;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, PageType, EntityId));
    }

    public void UpdateIndexingSettings(bool isIndexed, bool followLinks)
    {
        IsIndexed = isIndexed;
        FollowLinks = followLinks;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, PageType, EntityId));
    }

    public void UpdatePageType(string newPageType)
    {
        Guard.AgainstNullOrEmpty(newPageType, nameof(newPageType));
        // Configuration değeri: MaxPageTypeLength=50
        Guard.AgainstLength(newPageType, 50, nameof(newPageType));
        PageType = newPageType;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, newPageType, EntityId));
    }

    public void UpdateEntityId(Guid? newEntityId)
    {
        EntityId = newEntityId;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, PageType, newEntityId));
    }

    public void UpdateSitemapSettings(decimal priority, string? changeFrequency)
    {
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
        
        AddDomainEvent(new SEOSettingsUpdatedEvent(Id, PageType, EntityId));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SEOSettingsDeletedEvent(Id, PageType, EntityId));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SEOSettingsRestoredEvent(Id, PageType, EntityId));
    }

    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

