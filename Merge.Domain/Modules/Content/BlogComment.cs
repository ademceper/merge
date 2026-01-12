using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Identity;

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
    public ICollection<BlogComment> Replies { get; private set; } = new List<BlogComment>();

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
        Guard.AgainstNegative(content.Length, nameof(content));
        
        if (content.Length > 2000)
        {
            throw new DomainException("Yorum içeriği en fazla 2000 karakter olabilir.");
        }

        // Guest comment validation
        if (!userId.HasValue)
        {
            Guard.AgainstNullOrEmpty(authorName, nameof(authorName));
            Guard.AgainstNullOrEmpty(authorEmail, nameof(authorEmail));
            
            if (authorEmail != null && !authorEmail.Contains("@"))
            {
                throw new DomainException("Geçerli bir e-posta adresi giriniz.");
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
        
        if (newContent.Length > 2000)
        {
            throw new DomainException("Yorum içeriği en fazla 2000 karakter olabilir.");
        }

        Content = newContent;
        UpdatedAt = DateTime.UtcNow;
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

    // ✅ BOLUM 1.1: Domain Logic - Increment like count
    public void IncrementLikeCount()
    {
        LikeCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Decrement like count
    public void DecrementLikeCount()
    {
        if (LikeCount > 0)
        {
            LikeCount--;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BlogCommentDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BlogCommentDeletedEvent(Id, BlogPostId));
    }
}

