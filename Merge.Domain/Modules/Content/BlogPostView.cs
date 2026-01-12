using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// BlogPostView Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class BlogPostView : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid BlogPostId { get; private set; }
    public Guid? UserId { get; private set; } // Nullable for anonymous views
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public int ViewDurationSeconds { get; private set; } = 0; // How long user viewed the post

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public BlogPost BlogPost { get; private set; } = null!;
    public User? User { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private BlogPostView() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static BlogPostView Create(
        Guid blogPostId,
        Guid? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        int viewDurationSeconds = 0)
    {
        Guard.AgainstDefault(blogPostId, nameof(blogPostId));
        Guard.AgainstNegative(viewDurationSeconds, nameof(viewDurationSeconds));

        var view = new BlogPostView
        {
            Id = Guid.NewGuid(),
            BlogPostId = blogPostId,
            UserId = userId,
            IpAddress = ipAddress ?? string.Empty,
            UserAgent = userAgent ?? string.Empty,
            ViewDurationSeconds = viewDurationSeconds,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return view;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update view duration
    public void UpdateViewDuration(int durationSeconds)
    {
        Guard.AgainstNegative(durationSeconds, nameof(durationSeconds));
        ViewDurationSeconds = durationSeconds;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

