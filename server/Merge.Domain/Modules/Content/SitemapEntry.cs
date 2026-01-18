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
    public string Url { get; private set; } = string.Empty;
    public string PageType { get; private set; } = string.Empty; // Product, Category, Blog, Page
    public Guid? EntityId { get; private set; }
    public DateTime LastModified { get; private set; } = DateTime.UtcNow;
    public string ChangeFrequency { get; private set; } = "weekly"; // always, hourly, daily, weekly, monthly, yearly, never
    public decimal Priority { get; private set; } = 0.5m; // 0.0 to 1.0
    public bool IsActive { get; private set; } = true;

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private SitemapEntry() { }

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
        // Configuration değerleri: MinSitemapPriority=0.0, MaxSitemapPriority=1.0, MaxPageTypeLength=50
        Guard.AgainstOutOfRange(priority, 0m, 1m, nameof(priority));
        Guard.AgainstLength(pageType, 50, nameof(pageType));

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

        entry.AddDomainEvent(new SitemapEntryCreatedEvent(entry.Id, url, pageType));

        return entry;
    }

    public void UpdateUrl(string newUrl)
    {
        Guard.AgainstNullOrEmpty(newUrl, nameof(newUrl));
        
        if (!IsValidUrl(newUrl))
        {
            throw new DomainException("Geçerli bir URL giriniz.");
        }
        
        Url = newUrl;
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, newUrl));
    }

    public void UpdatePageType(string newPageType)
    {
        Guard.AgainstNullOrEmpty(newPageType, nameof(newPageType));
        // Configuration değeri: MaxPageTypeLength=50
        Guard.AgainstLength(newPageType, 50, nameof(newPageType));
        PageType = newPageType;
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, Url));
    }

    public void UpdateEntityId(Guid? newEntityId)
    {
        EntityId = newEntityId;
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, Url));
    }

    public void UpdateSitemapSettings(string changeFrequency, decimal priority)
    {
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
        
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, Url));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, Url));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, Url));
    }

    public void UpdateLastModified()
    {
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SitemapEntryUpdatedEvent(Id, Url));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SitemapEntryDeletedEvent(Id, Url));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SitemapEntryRestoredEvent(Id, Url));
    }

    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

