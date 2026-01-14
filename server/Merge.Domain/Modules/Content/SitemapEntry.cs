using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// SitemapEntry Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class SitemapEntry : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Url { get; private set; } = string.Empty;
    public string PageType { get; private set; } = string.Empty; // Product, Category, Blog, Page
    public Guid? EntityId { get; private set; }
    public DateTime LastModified { get; private set; } = DateTime.UtcNow;
    public string ChangeFrequency { get; private set; } = "weekly"; // always, hourly, daily, weekly, monthly, yearly, never
    public decimal Priority { get; private set; } = 0.5m; // 0.0 to 1.0
    public bool IsActive { get; private set; } = true;

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private SitemapEntry() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static SitemapEntry Create(
        string url,
        string pageType,
        Guid? entityId = null,
        string changeFrequency = "weekly",
        decimal priority = 0.5m,
        bool isActive = true)
    {
        Guard.AgainstNullOrEmpty(url, nameof(url));
        Guard.AgainstNullOrEmpty(pageType, nameof(pageType));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MinSitemapPriority=0.0, MaxSitemapPriority=1.0, MaxPageTypeLength=50
        Guard.AgainstOutOfRange(priority, 0m, 1m, nameof(priority));
        Guard.AgainstLength(pageType, 50, nameof(pageType));

        // ✅ BOLUM 1.3: URL Validation - Domain layer'da URL validasyonu
        if (!IsValidUrl(url))
        {
            throw new DomainException("Geçerli bir URL giriniz.");
        }

        var validChangeFrequencies = new[] { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" };
        if (!validChangeFrequencies.Contains(changeFrequency.ToLowerInvariant()))
        {
            throw new DomainException($"Geçersiz change frequency. Geçerli değerler: {string.Join(", ", validChangeFrequencies)}");
        }

        var entry = new SitemapEntry
        {
            Id = Guid.NewGuid(),
            Url = url,
            PageType = pageType,
            EntityId = entityId,
            LastModified = DateTime.UtcNow,
            ChangeFrequency = changeFrequency,
            Priority = priority,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - SitemapEntryCreatedEvent yayınla (ÖNERİLİR)
        entry.AddDomainEvent(new SitemapEntryCreatedEvent(entry.Id, url, pageType));

        return entry;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update URL
    public void UpdateUrl(string newUrl)
    {
        Guard.AgainstNullOrEmpty(newUrl, nameof(newUrl));
        
        // ✅ BOLUM 1.3: URL Validation - Domain layer'da URL validasyonu
        if (!IsValidUrl(newUrl))
        {
            throw new DomainException("Geçerli bir URL giriniz.");
        }
        
        Url = newUrl;
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SitemapEntryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, newUrl));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update page type
    public void UpdatePageType(string newPageType)
    {
        Guard.AgainstNullOrEmpty(newPageType, nameof(newPageType));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxPageTypeLength=50
        Guard.AgainstLength(newPageType, 50, nameof(newPageType));
        PageType = newPageType;
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SitemapEntryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, Url));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update entity ID
    public void UpdateEntityId(Guid? newEntityId)
    {
        EntityId = newEntityId;
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SitemapEntryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, Url));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update sitemap settings
    public void UpdateSitemapSettings(string changeFrequency, decimal priority)
    {
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MinSitemapPriority=0.0, MaxSitemapPriority=1.0
        Guard.AgainstOutOfRange(priority, 0m, 1m, nameof(priority));

        var validChangeFrequencies = new[] { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" };
        if (!validChangeFrequencies.Contains(changeFrequency.ToLowerInvariant()))
        {
            throw new DomainException($"Geçersiz change frequency. Geçerli değerler: {string.Join(", ", validChangeFrequencies)}");
        }

        ChangeFrequency = changeFrequency;
        Priority = priority;
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SitemapEntryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, Url));
    }

    // ✅ BOLUM 1.1: Domain Logic - Activate
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SitemapEntryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, Url));
    }

    // ✅ BOLUM 1.1: Domain Logic - Deactivate
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SitemapEntryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, Url));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update last modified
    public void UpdateLastModified()
    {
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SitemapEntryUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, Url));
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SitemapEntryDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SitemapEntryDeletedEvent(Id, Url));
    }

    // ✅ BOLUM 1.1: Domain Logic - Restore deleted sitemap entry
    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - SitemapEntryRestoredEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new SitemapEntryRestoredEvent(Id, Url));
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

