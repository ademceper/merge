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
    public Guid BlogPostId { get; private set; }
    public Guid? UserId { get; private set; } // Nullable for anonymous views
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public int ViewDurationSeconds { get; private set; } = 0; // How long user viewed the post

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public BlogPost BlogPost { get; private set; } = null!;
    public User? User { get; private set; }

    private BlogPostView() { }

    public static BlogPostView Create(
        Guid blogPostId,
        Guid? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        int viewDurationSeconds = 0)
    {
        Guard.AgainstDefault(blogPostId, nameof(blogPostId));
        Guard.AgainstNegative(viewDurationSeconds, nameof(viewDurationSeconds));
        // Configuration değerleri: MaxIpAddressLength=45, MaxUserAgentLength=500
        if (!string.IsNullOrEmpty(ipAddress))
            Guard.AgainstLength(ipAddress, 45, nameof(ipAddress));
        if (!string.IsNullOrEmpty(userAgent))
            Guard.AgainstLength(userAgent, 500, nameof(userAgent));

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

    public void UpdateViewDuration(int durationSeconds)
    {
        Guard.AgainstNegative(durationSeconds, nameof(durationSeconds));
        ViewDurationSeconds = durationSeconds;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

