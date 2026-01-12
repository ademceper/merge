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
/// BlogPost Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class BlogPost : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid CategoryId { get; private set; }
    public BlogCategory Category { get; private set; } = null!;
    public Guid AuthorId { get; private set; }
    public User Author { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı (ZORUNLU)
    public Slug Slug { get; private set; } = null!;
    public string Excerpt { get; private set; } = string.Empty; // Short summary
    public string Content { get; private set; } = string.Empty; // Full content (HTML/Markdown)
    public string? FeaturedImageUrl { get; private set; }
    public ContentStatus Status { get; private set; } = ContentStatus.Draft;
    public DateTime? PublishedAt { get; private set; }
    public int ViewCount { get; private set; } = 0;
    public int LikeCount { get; private set; } = 0;
    public int CommentCount { get; private set; } = 0;
    public string? Tags { get; private set; } // Comma separated tags
    public bool IsFeatured { get; private set; } = false;
    public bool AllowComments { get; private set; } = true;
    public string? MetaTitle { get; private set; } // SEO meta title
    public string? MetaDescription { get; private set; } // SEO meta description
    public string? MetaKeywords { get; private set; } // SEO keywords
    public string? OgImageUrl { get; private set; } // Open Graph image for social sharing
    public int ReadingTimeMinutes { get; private set; } = 0; // Estimated reading time

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // ✅ BOLUM 1.1: Encapsulated collection - Read-only access
    private readonly List<BlogComment> _comments = new();
    public IReadOnlyCollection<BlogComment> Comments => _comments.AsReadOnly();
    
    private readonly List<BlogPostView> _views = new();
    public IReadOnlyCollection<BlogPostView> Views => _views.AsReadOnly();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private BlogPost() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static BlogPost Create(
        Guid categoryId,
        Guid authorId,
        string title,
        string excerpt,
        string content,
        string? featuredImageUrl = null,
        ContentStatus status = ContentStatus.Draft,
        string? tags = null,
        bool isFeatured = false,
        bool allowComments = true,
        string? metaTitle = null,
        string? metaDescription = null,
        string? metaKeywords = null,
        string? ogImageUrl = null,
        int readingTimeMinutes = 0,
        string? slug = null) // Optional slug for uniqueness handling
    {
        Guard.AgainstDefault(categoryId, nameof(categoryId));
        Guard.AgainstDefault(authorId, nameof(authorId));
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstNullOrEmpty(excerpt, nameof(excerpt));
        Guard.AgainstNullOrEmpty(content, nameof(content));
        Guard.AgainstNegative(readingTimeMinutes, nameof(readingTimeMinutes));

        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        var finalSlug = slug != null ? Slug.FromString(slug) : Slug.FromString(title);

        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            AuthorId = authorId,
            Title = title,
            Slug = finalSlug,
            Excerpt = excerpt,
            Content = content,
            FeaturedImageUrl = featuredImageUrl,
            Status = status,
            Tags = tags,
            IsFeatured = isFeatured,
            AllowComments = allowComments,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords,
            OgImageUrl = ogImageUrl,
            ReadingTimeMinutes = readingTimeMinutes,
            PublishedAt = status == ContentStatus.Published ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - BlogPostCreatedEvent yayınla (ÖNERİLİR)
        post.AddDomainEvent(new BlogPostCreatedEvent(post.Id, title, finalSlug.Value, authorId, categoryId));

        return post;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update title
    public void UpdateTitle(string newTitle)
    {
        Guard.AgainstNullOrEmpty(newTitle, nameof(newTitle));
        Title = newTitle;
        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        Slug = Slug.FromString(newTitle);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostUpdatedEvent(Id, newTitle, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update excerpt
    public void UpdateExcerpt(string newExcerpt)
    {
        Guard.AgainstNullOrEmpty(newExcerpt, nameof(newExcerpt));
        Excerpt = newExcerpt;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update content
    public void UpdateContent(string newContent)
    {
        Guard.AgainstNullOrEmpty(newContent, nameof(newContent));
        Content = newContent;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update featured image
    public void UpdateFeaturedImage(string? newFeaturedImageUrl)
    {
        FeaturedImageUrl = newFeaturedImageUrl;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update category
    public void UpdateCategory(Guid newCategoryId)
    {
        Guard.AgainstDefault(newCategoryId, nameof(newCategoryId));
        CategoryId = newCategoryId;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update status
    public void UpdateStatus(ContentStatus newStatus)
    {
        Status = newStatus;
        if (newStatus == ContentStatus.Published && !PublishedAt.HasValue)
        {
            PublishedAt = DateTime.UtcNow;
            // ✅ BOLUM 1.5: Domain Events - BlogPostPublishedEvent yayınla (ÖNERİLİR)
            AddDomainEvent(new BlogPostPublishedEvent(Id, Title, Slug.Value, AuthorId));
        }
        else
        {
            // ✅ BOLUM 1.5: Domain Events - BlogPostUpdatedEvent yayınla (ÖNERİLİR)
            AddDomainEvent(new BlogPostUpdatedEvent(Id, Title, Slug.Value));
        }
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Publish post
    public void Publish()
    {
        if (Status == ContentStatus.Published)
            return;

        Status = ContentStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostPublishedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostPublishedEvent(Id, Title, Slug.Value, AuthorId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Unpublish post
    public void Unpublish()
    {
        if (Status == ContentStatus.Draft)
            return;

        Status = ContentStatus.Draft;
        PublishedAt = null;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostUnpublishedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostUnpublishedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update tags
    public void UpdateTags(string? newTags)
    {
        Tags = newTags;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Set as featured
    public void SetAsFeatured()
    {
        if (IsFeatured)
            return;

        IsFeatured = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Unset as featured
    public void UnsetAsFeatured()
    {
        if (!IsFeatured)
            return;

        IsFeatured = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update allow comments
    public void UpdateAllowComments(bool allowComments)
    {
        AllowComments = allowComments;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update meta information
    public void UpdateMetaInformation(string? metaTitle, string? metaDescription, string? metaKeywords, string? ogImageUrl)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
        OgImageUrl = ogImageUrl;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update reading time
    public void UpdateReadingTime(int readingTimeMinutes)
    {
        Guard.AgainstNegative(readingTimeMinutes, nameof(readingTimeMinutes));
        ReadingTimeMinutes = readingTimeMinutes;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Increment view count
    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostViewedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostViewedEvent(Id, Title, ViewCount));
    }

    // ✅ BOLUM 1.1: Domain Logic - Increment like count
    public void IncrementLikeCount()
    {
        LikeCount++;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostLikedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostLikedEvent(Id, Title, LikeCount));
    }

    // ✅ BOLUM 1.1: Domain Logic - Decrement like count
    public void DecrementLikeCount()
    {
        if (LikeCount > 0)
        {
            LikeCount--;
            UpdatedAt = DateTime.UtcNow;
            
            // ✅ BOLUM 1.5: Domain Events - BlogPostUnlikedEvent yayınla (ÖNERİLİR)
            AddDomainEvent(new BlogPostUnlikedEvent(Id, Title, LikeCount));
        }
    }

    // ✅ BOLUM 1.1: Domain Logic - Increment comment count
    public void IncrementCommentCount()
    {
        CommentCount++;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostCommentCountUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostCommentCountUpdatedEvent(Id, Title, CommentCount));
    }

    // ✅ BOLUM 1.1: Domain Logic - Decrement comment count
    public void DecrementCommentCount()
    {
        if (CommentCount > 0)
        {
            CommentCount--;
            UpdatedAt = DateTime.UtcNow;
            
            // ✅ BOLUM 1.5: Domain Events - BlogPostCommentCountUpdatedEvent yayınla (ÖNERİLİR)
            AddDomainEvent(new BlogPostCommentCountUpdatedEvent(Id, Title, CommentCount));
        }
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogPostDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogPostDeletedEvent(Id, Title));
    }

}

