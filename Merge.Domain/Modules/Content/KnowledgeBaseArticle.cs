using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// KnowledgeBaseArticle Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class KnowledgeBaseArticle : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Title { get; private set; } = string.Empty;
    // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı (ZORUNLU)
    public Slug Slug { get; private set; } = null!;
    public string Content { get; private set; } = string.Empty;
    public string? Excerpt { get; private set; }
    public Guid? CategoryId { get; private set; }
    public ContentStatus Status { get; private set; } = ContentStatus.Draft;
    public int ViewCount { get; private set; } = 0;
    public int HelpfulCount { get; private set; } = 0;
    public int NotHelpfulCount { get; private set; } = 0;
    public bool IsFeatured { get; private set; } = false;
    public int DisplayOrder { get; private set; } = 0;
    public string? Tags { get; private set; }
    public Guid? AuthorId { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    // Navigation properties - EF Core requires setters, but we keep them private for encapsulation
    public KnowledgeBaseCategory? Category { get; private set; }
    public User? Author { get; private set; }
    
    // ✅ BOLUM 1.1: Encapsulated collection - Read-only access
    private readonly List<KnowledgeBaseView> _views = new();
    public IReadOnlyCollection<KnowledgeBaseView> Views => _views.AsReadOnly();

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private KnowledgeBaseArticle() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static KnowledgeBaseArticle Create(
        string title,
        string slug,
        string content,
        Guid? authorId,
        Guid? categoryId = null,
        string? excerpt = null,
        ContentStatus status = ContentStatus.Draft,
        bool isFeatured = false,
        int displayOrder = 0,
        string? tags = null)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstNullOrEmpty(slug, nameof(slug));
        Guard.AgainstNullOrEmpty(content, nameof(content));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxArticleTitleLength=200, MaxArticleContentLength=50000, MaxArticleExcerptLength=500
        Guard.AgainstLength(title, 200, nameof(title));
        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        var slugValueObject = Slug.FromString(slug);
        Guard.AgainstLength(content, 50000, nameof(content));
        if (excerpt != null)
            Guard.AgainstLength(excerpt, 500, nameof(excerpt));

        var article = new KnowledgeBaseArticle
        {
            Id = Guid.NewGuid(),
            Title = title,
            Slug = slugValueObject,
            Content = content,
            Excerpt = excerpt,
            CategoryId = categoryId,
            Status = status,
            AuthorId = authorId,
            IsFeatured = isFeatured,
            DisplayOrder = displayOrder,
            Tags = tags,
            ViewCount = 0,
            HelpfulCount = 0,
            NotHelpfulCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (status == ContentStatus.Published)
        {
            article.PublishedAt = DateTime.UtcNow;
        }

        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleCreatedEvent
        article.AddDomainEvent(new KnowledgeBaseArticleCreatedEvent(
            article.Id,
            article.Title,
            article.Slug.Value,
            authorId,
            categoryId));

        return article;
    }

    // ✅ BOLUM 1.1: Domain Method - Publish article
    public void Publish()
    {
        if (Status == ContentStatus.Published)
            return;

        Status = ContentStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticlePublishedEvent
        AddDomainEvent(new KnowledgeBaseArticlePublishedEvent(Id, Title, Slug.Value, PublishedAt.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Unpublish article
    public void Unpublish()
    {
        if (Status == ContentStatus.Draft)
            return;

        Status = ContentStatus.Draft;
        PublishedAt = null;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleUnpublishedEvent
        AddDomainEvent(new KnowledgeBaseArticleUnpublishedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Update title and slug
    public void UpdateTitle(string title, string slug)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstNullOrEmpty(slug, nameof(slug));
        Guard.AgainstLength(title, 200, nameof(title));
        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        var slugValueObject = Slug.FromString(slug);

        Title = title;
        Slug = slugValueObject;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, title, slugValueObject.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Update content
    public void UpdateContent(string content, string? excerpt = null)
    {
        Guard.AgainstNullOrEmpty(content, nameof(content));
        Guard.AgainstLength(content, 50000, nameof(content));
        if (excerpt != null)
            Guard.AgainstLength(excerpt, 500, nameof(excerpt));

        Content = content;
        Excerpt = excerpt;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Update category
    public void UpdateCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as featured
    public void SetFeatured(bool isFeatured)
    {
        IsFeatured = isFeatured;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Increment view count
    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleViewedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseArticleViewedEvent(Id, Title, ViewCount));
    }

    // ✅ BOLUM 1.1: Domain Method - Record view (creates KnowledgeBaseView and increments count)
    public KnowledgeBaseView RecordView(Guid? userId = null, string? ipAddress = null, string? userAgent = null)
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleViewedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseArticleViewedEvent(Id, Title, ViewCount));

        var view = KnowledgeBaseView.Create(
            Id,
            userId,
            ipAddress,
            userAgent,
            0);

        return view;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as helpful
    public void MarkAsHelpful()
    {
        HelpfulCount++;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleMarkedAsHelpfulEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseArticleMarkedAsHelpfulEvent(Id, Title, HelpfulCount));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as not helpful
    public void MarkAsNotHelpful()
    {
        NotHelpfulCount++;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleMarkedAsNotHelpfulEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseArticleMarkedAsNotHelpfulEvent(Id, Title, NotHelpfulCount));
    }

    // ✅ BOLUM 1.1: Domain Method - Update display order
    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Update tags
    public void UpdateTags(string? tags)
    {
        Tags = tags;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Update status (for non-published statuses)
    public void UpdateStatus(ContentStatus status)
    {
        if (Status == status) return;

        if (status == ContentStatus.Published)
        {
            Publish(); // Use Publish method for published status
            return;
        }

        Status = status;
        if (status != ContentStatus.Published)
        {
            PublishedAt = null; // Clear published date if not published
        }
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - KnowledgeBaseArticleDeletedEvent
        AddDomainEvent(new KnowledgeBaseArticleDeletedEvent(Id, Title, CategoryId));
    }
}

