using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// PageBuilder Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class PageBuilder : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı (ZORUNLU)
    public Slug Slug { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty; // JSON content for page builder
    public string? Template { get; private set; } // Template identifier
    public ContentStatus Status { get; private set; } = ContentStatus.Draft;
    public Guid? AuthorId { get; private set; }
    public User? Author { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public string? OgImageUrl { get; private set; }
    public int ViewCount { get; private set; } = 0;
    public string? PageType { get; private set; } // Home, Category, Product, Custom
    public Guid? RelatedEntityId { get; private set; } // Related category/product ID if applicable

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private PageBuilder() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static PageBuilder Create(
        string name,
        string title,
        string content,
        Guid? authorId = null,
        string? slug = null,
        string? template = null,
        ContentStatus status = ContentStatus.Draft,
        string? pageType = null,
        Guid? relatedEntityId = null,
        string? metaTitle = null,
        string? metaDescription = null,
        string? ogImageUrl = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstNullOrEmpty(content, nameof(content));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxPageBuilderNameLength=200, MaxPageBuilderTitleLength=200, MaxPageBuilderContentLength=50000, MaxPageTypeLength=50
        Guard.AgainstLength(name, 200, nameof(name));
        Guard.AgainstLength(title, 200, nameof(title));
        Guard.AgainstLength(content, 50000, nameof(content));
        if (!string.IsNullOrEmpty(pageType))
            Guard.AgainstLength(pageType, 50, nameof(pageType));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxTemplateNameLength=100, MaxMetaTitleLength=60, MaxMetaDescriptionLength=160
        if (template != null)
            Guard.AgainstLength(template, 100, nameof(template));
        if (metaTitle != null)
            Guard.AgainstLength(metaTitle, 60, nameof(metaTitle));
        if (metaDescription != null)
            Guard.AgainstLength(metaDescription, 160, nameof(metaDescription));

        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        var finalSlug = slug != null ? Slug.FromString(slug) : Slug.FromString(name);

        // ✅ BOLUM 1.3: URL Validation - Domain layer'da URL validasyonu
        if (!string.IsNullOrEmpty(ogImageUrl) && !IsValidUrl(ogImageUrl))
        {
            throw new DomainException("Geçerli bir Open Graph image URL giriniz.");
        }

        var pageBuilder = new PageBuilder
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = finalSlug,
            Title = title,
            Content = content,
            Template = template,
            Status = status,
            AuthorId = authorId,
            PageType = pageType,
            RelatedEntityId = relatedEntityId,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            OgImageUrl = ogImageUrl,
            IsActive = true,
            PublishedAt = status == ContentStatus.Published ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events (ÖNERİLİR)
        pageBuilder.AddDomainEvent(new PageBuilderCreatedEvent(
            pageBuilder.Id,
            pageBuilder.Name,
            pageBuilder.Slug.Value,
            pageBuilder.AuthorId ?? Guid.Empty));

        return pageBuilder;
    }

    // ✅ BOLUM 1.1: Domain Methods - Business logic encapsulation
    public void UpdateName(string name)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxPageBuilderNameLength=200
        Guard.AgainstLength(name, 200, nameof(name));
        Name = name;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageBuilderUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateSlug(string slug)
    {
        Guard.AgainstNullOrEmpty(slug, nameof(slug));
        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        Slug = Slug.FromString(slug);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageBuilderUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateTitle(string title)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxPageBuilderTitleLength=200
        Guard.AgainstLength(title, 200, nameof(title));
        Title = title;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageBuilderUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateContent(string content)
    {
        Guard.AgainstNullOrEmpty(content, nameof(content));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxPageBuilderContentLength=50000
        Guard.AgainstLength(content, 50000, nameof(content));
        Content = content;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageBuilderUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateTemplate(string? template)
    {
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxTemplateNameLength=100
        if (template != null)
            Guard.AgainstLength(template, 100, nameof(template));
        Template = template;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageBuilderUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateStatus(ContentStatus status)
    {
        if (Status == status) return;

        Status = status;
        UpdatedAt = DateTime.UtcNow;

        if (status == ContentStatus.Published && !PublishedAt.HasValue)
        {
            PublishedAt = DateTime.UtcNow;
            IsActive = true;
            AddDomainEvent(new PageBuilderPublishedEvent(Id, Name, Slug.Value, AuthorId ?? Guid.Empty));
        }
        else if (status == ContentStatus.Draft)
        {
            IsActive = false;
            AddDomainEvent(new PageBuilderUpdatedEvent(Id, Name, Slug.Value));
        }
        else
        {
            AddDomainEvent(new PageBuilderUpdatedEvent(Id, Name, Slug.Value));
        }
    }

    public void UpdatePageType(string? pageType)
    {
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxPageTypeLength=50
        if (!string.IsNullOrEmpty(pageType))
            Guard.AgainstLength(pageType, 50, nameof(pageType));
        PageType = pageType;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageBuilderUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateRelatedEntity(Guid? relatedEntityId)
    {
        RelatedEntityId = relatedEntityId;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageBuilderUpdatedEvent(Id, Name, Slug.Value));
    }

    public void UpdateMetaInformation(string? metaTitle, string? metaDescription, string? ogImageUrl)
    {
        // ✅ BOLUM 1.3: URL Validation - Domain layer'da URL validasyonu
        if (!string.IsNullOrEmpty(ogImageUrl) && !IsValidUrl(ogImageUrl))
        {
            throw new DomainException("Geçerli bir Open Graph image URL giriniz.");
        }
        
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxMetaTitleLength=60, MaxMetaDescriptionLength=160
        if (metaTitle != null)
            Guard.AgainstLength(metaTitle, 60, nameof(metaTitle));
        if (metaDescription != null)
            Guard.AgainstLength(metaDescription, 160, nameof(metaDescription));
        
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        OgImageUrl = ogImageUrl;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageBuilderUpdatedEvent(Id, Name, Slug.Value));
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageBuilderUpdatedEvent(Id, Name, Slug.Value));
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageBuilderUpdatedEvent(Id, Name, Slug.Value));
    }

    public void Publish()
    {
        if (Status == ContentStatus.Published) return;

        Status = ContentStatus.Published;
        PublishedAt = DateTime.UtcNow;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PageBuilderPublishedEvent(Id, Name, Slug.Value, AuthorId ?? Guid.Empty));
    }

    public void Unpublish()
    {
        if (Status == ContentStatus.Draft) return;

        Status = ContentStatus.Draft;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PageBuilderUnpublishedEvent(Id, Name, Slug.Value));
    }

    public void UpdateAuthor(Guid? newAuthorId)
    {
        AuthorId = newAuthorId;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageBuilderUpdatedEvent(Id, Name, Slug.Value));
    }

    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PageBuilderViewedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new PageBuilderViewedEvent(Id, Name, ViewCount));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageBuilderDeletedEvent(Id, Name, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Restore deleted page builder
    public void Restore()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageBuilderRestoredEvent(Id, Name, Slug.Value));
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
