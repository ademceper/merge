using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// BlogComment Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class BlogComment : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid BlogPostId { get; private set; }
    public Guid? UserId { get; private set; } // Nullable for guest comments
    public Guid? ParentCommentId { get; private set; } // For nested comments/replies
    public string AuthorName { get; private set; } = string.Empty; // For guest comments
    public string AuthorEmail { get; private set; } = string.Empty; // For guest comments
    public string Content { get; private set; } = string.Empty;
    public bool IsApproved { get; private set; } = false;
    public int LikeCount { get; private set; } = 0;

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public BlogPost BlogPost { get; private set; } = null!;
    public User? User { get; private set; }
    public BlogComment? ParentComment { get; private set; }
    
    // ✅ BOLUM 1.1: Encapsulated collection - Read-only access
    private readonly List<BlogComment> _replies = new();
    public IReadOnlyCollection<BlogComment> Replies => _replies.AsReadOnly();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private BlogComment() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static BlogComment Create(
        Guid blogPostId,
        string content,
        Guid? userId = null,
        Guid? parentCommentId = null,
        string? authorName = null,
        string? authorEmail = null,
        bool autoApprove = false)
    {
        Guard.AgainstDefault(blogPostId, nameof(blogPostId));
        Guard.AgainstNullOrEmpty(content, nameof(content));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxCommentContentLength=2000, MaxCommentAuthorNameLength=100
        Guard.AgainstLength(content, 2000, nameof(content));

        // Guest comment validation
        if (!userId.HasValue)
        {
            Guard.AgainstNullOrEmpty(authorName, nameof(authorName));
            Guard.AgainstNullOrEmpty(authorEmail, nameof(authorEmail));
            Guard.AgainstLength(authorName!, 100, nameof(authorName));
            
            // ✅ BOLUM 1.3: Value Objects - Email validation using Email Value Object
            try
            {
                var email = new Email(authorEmail!);
                authorEmail = email.Value; // Normalize email
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Geçerli bir e-posta adresi giriniz: {ex.Message}");
            }
        }

        var comment = new BlogComment
        {
            Id = Guid.NewGuid(),
            BlogPostId = blogPostId,
            UserId = userId,
            ParentCommentId = parentCommentId,
            AuthorName = authorName ?? string.Empty,
            AuthorEmail = authorEmail ?? string.Empty,
            Content = content,
            IsApproved = autoApprove || userId.HasValue, // Auto-approve for logged-in users
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - BlogCommentCreatedEvent yayınla (ÖNERİLİR)
        comment.AddDomainEvent(new BlogCommentCreatedEvent(comment.Id, blogPostId, userId));

        return comment;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update content
    public void UpdateContent(string newContent)
    {
        Guard.AgainstNullOrEmpty(newContent, nameof(newContent));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxCommentContentLength=2000
        Guard.AgainstLength(newContent, 2000, nameof(newContent));

        Content = newContent;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCommentUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCommentUpdatedEvent(Id, BlogPostId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update author name (for guest comments)
    public void UpdateAuthorName(string newAuthorName)
    {
        if (UserId.HasValue)
        {
            throw new DomainException("Logged-in user comments cannot have author name updated.");
        }

        Guard.AgainstNullOrEmpty(newAuthorName, nameof(newAuthorName));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxCommentAuthorNameLength=100
        Guard.AgainstLength(newAuthorName, 100, nameof(newAuthorName));
        AuthorName = newAuthorName;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCommentUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCommentUpdatedEvent(Id, BlogPostId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update author email (for guest comments)
    public void UpdateAuthorEmail(string newAuthorEmail)
    {
        if (UserId.HasValue)
        {
            throw new DomainException("Logged-in user comments cannot have author email updated.");
        }

        Guard.AgainstNullOrEmpty(newAuthorEmail, nameof(newAuthorEmail));
        
        // ✅ BOLUM 1.3: Value Objects - Email validation using Email Value Object
        try
        {
            var email = new Email(newAuthorEmail);
            AuthorEmail = email.Value; // Normalize email
        }
        catch (ArgumentException ex)
        {
            throw new DomainException($"Geçerli bir e-posta adresi giriniz: {ex.Message}");
        }

        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCommentUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCommentUpdatedEvent(Id, BlogPostId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update parent comment (for nested comments)
    public void UpdateParentComment(Guid? newParentCommentId)
    {
        if (newParentCommentId.HasValue && newParentCommentId.Value == Id)
        {
            throw new DomainException("Comment cannot be its own parent.");
        }

        ParentCommentId = newParentCommentId;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCommentUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCommentUpdatedEvent(Id, BlogPostId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Approve comment
    public void Approve()
    {
        if (IsApproved)
            return;

        IsApproved = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCommentApprovedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCommentApprovedEvent(Id, BlogPostId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Disapprove comment
    public void Disapprove()
    {
        if (!IsApproved)
            return;

        IsApproved = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCommentDisapprovedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCommentDisapprovedEvent(Id, BlogPostId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Increment like count
    public void IncrementLikeCount()
    {
        LikeCount++;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCommentLikedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCommentLikedEvent(Id, BlogPostId, LikeCount));
    }

    // ✅ BOLUM 1.1: Domain Logic - Decrement like count
    public void DecrementLikeCount()
    {
        if (LikeCount > 0)
        {
            LikeCount--;
            UpdatedAt = DateTime.UtcNow;
            
            // ✅ BOLUM 1.5: Domain Events - BlogCommentUnlikedEvent yayınla (ÖNERİLİR)
            AddDomainEvent(new BlogCommentUnlikedEvent(Id, BlogPostId, LikeCount));
        }
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCommentDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCommentDeletedEvent(Id, BlogPostId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Restore deleted comment
    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCommentRestoredEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCommentRestoredEvent(Id, BlogPostId));
    }
}

