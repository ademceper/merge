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
    public Guid BlogPostId { get; private set; }
    public Guid? UserId { get; private set; } // Nullable for guest comments
    public Guid? ParentCommentId { get; private set; } // For nested comments/replies
    public string AuthorName { get; private set; } = string.Empty; // For guest comments
    public string AuthorEmail { get; private set; } = string.Empty; // For guest comments
    public string Content { get; private set; } = string.Empty;
    public bool IsApproved { get; private set; } = false;
    public int LikeCount { get; private set; } = 0;

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public BlogPost BlogPost { get; private set; } = null!;
    public User? User { get; private set; }
    public BlogComment? ParentComment { get; private set; }
    
    private readonly List<BlogComment> _replies = new();
    public IReadOnlyCollection<BlogComment> Replies => _replies.AsReadOnly();

    private BlogComment() { }

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
        // Configuration değerleri: MaxCommentContentLength=2000, MaxCommentAuthorNameLength=100
        Guard.AgainstLength(content, 2000, nameof(content));

        // Guest comment validation
        if (!userId.HasValue)
        {
            Guard.AgainstNullOrEmpty(authorName, nameof(authorName));
            Guard.AgainstNullOrEmpty(authorEmail, nameof(authorEmail));
            Guard.AgainstLength(authorName!, 100, nameof(authorName));
            
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

        comment.AddDomainEvent(new BlogCommentCreatedEvent(comment.Id, blogPostId, userId));

        return comment;
    }

    public void UpdateContent(string newContent)
    {
        Guard.AgainstNullOrEmpty(newContent, nameof(newContent));
        // Configuration değeri: MaxCommentContentLength=2000
        Guard.AgainstLength(newContent, 2000, nameof(newContent));

        Content = newContent;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCommentUpdatedEvent(Id, BlogPostId));
    }

    public void UpdateAuthorName(string newAuthorName)
    {
        if (UserId.HasValue)
        {
            throw new DomainException("Logged-in user comments cannot have author name updated.");
        }

        Guard.AgainstNullOrEmpty(newAuthorName, nameof(newAuthorName));
        // Configuration değeri: MaxCommentAuthorNameLength=100
        Guard.AgainstLength(newAuthorName, 100, nameof(newAuthorName));
        AuthorName = newAuthorName;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCommentUpdatedEvent(Id, BlogPostId));
    }

    public void UpdateAuthorEmail(string newAuthorEmail)
    {
        if (UserId.HasValue)
        {
            throw new DomainException("Logged-in user comments cannot have author email updated.");
        }

        Guard.AgainstNullOrEmpty(newAuthorEmail, nameof(newAuthorEmail));
        
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
        
        AddDomainEvent(new BlogCommentUpdatedEvent(Id, BlogPostId));
    }

    public void UpdateParentComment(Guid? newParentCommentId)
    {
        if (newParentCommentId.HasValue && newParentCommentId.Value == Id)
        {
            throw new DomainException("Comment cannot be its own parent.");
        }

        ParentCommentId = newParentCommentId;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCommentUpdatedEvent(Id, BlogPostId));
    }

    public void Approve()
    {
        if (IsApproved)
            return;

        IsApproved = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCommentApprovedEvent(Id, BlogPostId));
    }

    public void Disapprove()
    {
        if (!IsApproved)
            return;

        IsApproved = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCommentDisapprovedEvent(Id, BlogPostId));
    }

    public void IncrementLikeCount()
    {
        LikeCount++;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCommentLikedEvent(Id, BlogPostId, LikeCount));
    }

    public void DecrementLikeCount()
    {
        if (LikeCount > 0)
        {
            LikeCount--;
            UpdatedAt = DateTime.UtcNow;
            
            AddDomainEvent(new BlogCommentUnlikedEvent(Id, BlogPostId, LikeCount));
        }
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCommentDeletedEvent(Id, BlogPostId));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BlogCommentRestoredEvent(Id, BlogPostId));
    }
}

