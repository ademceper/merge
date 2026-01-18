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
    public string Title { get; private set; } = string.Empty;
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
    
    private readonly List<KnowledgeBaseView> _views = new();
    public IReadOnlyCollection<KnowledgeBaseView> Views => _views.AsReadOnly();

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private KnowledgeBaseArticle() { }

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
        // Configuration değerleri: MaxArticleTitleLength=200, MaxArticleContentLength=50000, MaxArticleExcerptLength=500
        Guard.AgainstLength(title, 200, nameof(title));
        var slugValueObject = Slug.FromString(slug);
        Guard.AgainstLength(content, 50000, nameof(content));
        if (excerpt != null)
            Guard.AgainstLength(excerpt, 500, nameof(excerpt));
        Guard.AgainstNegative(displayOrder, nameof(displayOrder));

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

        article.AddDomainEvent(new KnowledgeBaseArticleCreatedEvent(
            article.Id,
            article.Title,
            article.Slug.Value,
            authorId,
            categoryId));

        return article;
    }

    public void Publish()
    {
        if (Status == ContentStatus.Published)
            return;

        Status = ContentStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KnowledgeBaseArticlePublishedEvent(Id, Title, Slug.Value, PublishedAt.Value));
    }

    public void Unpublish()
    {
        if (Status == ContentStatus.Draft)
            return;

        Status = ContentStatus.Draft;
        PublishedAt = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KnowledgeBaseArticleUnpublishedEvent(Id, Title, Slug.Value));
    }

    public void UpdateTitle(string title, string slug)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstNullOrEmpty(slug, nameof(slug));
        Guard.AgainstLength(title, 200, nameof(title));
        var slugValueObject = Slug.FromString(slug);

        Title = title;
        Slug = slugValueObject;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, title, slugValueObject.Value));
    }

    public void UpdateTitle(string title)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstLength(title, 200, nameof(title));
        Title = title;
        Slug = Slug.FromString(title);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, title, Slug.Value));
    }

    public void UpdateSlug(string slug)
    {
        Guard.AgainstNullOrEmpty(slug, nameof(slug));
        Slug = Slug.FromString(slug);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateContent(string content, string? excerpt = null)
    {
        Guard.AgainstNullOrEmpty(content, nameof(content));
        Guard.AgainstLength(content, 50000, nameof(content));
        if (excerpt != null)
            Guard.AgainstLength(excerpt, 500, nameof(excerpt));

        Content = content;
        Excerpt = excerpt;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateExcerpt(string? excerpt)
    {
        // Configuration değeri: MaxArticleExcerptLength=500
        if (excerpt != null)
            Guard.AgainstLength(excerpt, 500, nameof(excerpt));

        Excerpt = excerpt;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateAuthor(Guid? authorId)
    {
        AuthorId = authorId;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    public void SetAsFeatured()
    {
        if (IsFeatured)
            return;

        IsFeatured = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UnsetAsFeatured()
    {
        if (!IsFeatured)
            return;

        IsFeatured = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleViewedEvent(Id, Title, ViewCount));
    }

    public KnowledgeBaseView RecordView(Guid? userId = null, string? ipAddress = null, string? userAgent = null)
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KnowledgeBaseArticleViewedEvent(Id, Title, ViewCount));

        var view = KnowledgeBaseView.Create(
            Id,
            userId,
            ipAddress,
            userAgent,
            0);

        return view;
    }

    public void MarkAsHelpful()
    {
        HelpfulCount++;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleMarkedAsHelpfulEvent(Id, Title, HelpfulCount));
    }

    public void MarkAsNotHelpful()
    {
        NotHelpfulCount++;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleMarkedAsNotHelpfulEvent(Id, Title, NotHelpfulCount));
    }

    public void DecrementHelpfulCount()
    {
        if (HelpfulCount > 0)
        {
            HelpfulCount--;
            UpdatedAt = DateTime.UtcNow;
            
            AddDomainEvent(new KnowledgeBaseArticleMarkedAsHelpfulEvent(Id, Title, HelpfulCount));
        }
    }

    public void DecrementNotHelpfulCount()
    {
        if (NotHelpfulCount > 0)
        {
            NotHelpfulCount--;
            UpdatedAt = DateTime.UtcNow;
            
            AddDomainEvent(new KnowledgeBaseArticleMarkedAsNotHelpfulEvent(Id, Title, NotHelpfulCount));
        }
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        Guard.AgainstNegative(displayOrder, nameof(displayOrder));
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateTags(string? tags)
    {
        Tags = tags;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

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
        
        AddDomainEvent(new KnowledgeBaseArticleUpdatedEvent(Id, Title, Slug.Value));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KnowledgeBaseArticleDeletedEvent(Id, Title, CategoryId));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KnowledgeBaseArticleRestoredEvent(Id, Title, Slug.Value));
    }
}

