using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

public class BlogCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public BlogCategory? ParentCategory { get; set; }
    public ICollection<BlogCategory> SubCategories { get; set; } = new List<BlogCategory>();
    public ICollection<BlogPost> Posts { get; set; } = new List<BlogPost>();
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class BlogPost : BaseEntity
{
    public Guid CategoryId { get; set; }
    public BlogCategory Category { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty; // Short summary
    public string Content { get; set; } = string.Empty; // Full content (HTML/Markdown)
    public string? FeaturedImageUrl { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; } = 0;
    public int LikeCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public string? Tags { get; set; } // Comma separated tags
    public bool IsFeatured { get; set; } = false;
    public bool AllowComments { get; set; } = true;
    public string? MetaTitle { get; set; } // SEO meta title
    public string? MetaDescription { get; set; } // SEO meta description
    public string? MetaKeywords { get; set; } // SEO keywords
    public string? OgImageUrl { get; set; } // Open Graph image for social sharing
    public int ReadingTimeMinutes { get; set; } = 0; // Estimated reading time
    
    // Navigation properties
    public ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
    public ICollection<BlogPostView> Views { get; set; } = new List<BlogPostView>();
}

public class BlogComment : BaseEntity
{
    public Guid BlogPostId { get; set; }
    public BlogPost BlogPost { get; set; } = null!;
    public Guid? UserId { get; set; } // Nullable for guest comments
    public User? User { get; set; }
    public Guid? ParentCommentId { get; set; } // For nested comments/replies
    public BlogComment? ParentComment { get; set; }
    public ICollection<BlogComment> Replies { get; set; } = new List<BlogComment>();
    public string AuthorName { get; set; } = string.Empty; // For guest comments
    public string AuthorEmail { get; set; } = string.Empty; // For guest comments
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = false;
    public int LikeCount { get; set; } = 0;
    
    // Navigation properties
}

public class BlogPostView : BaseEntity
{
    public Guid BlogPostId { get; set; }
    public BlogPost BlogPost { get; set; } = null!;
    public Guid? UserId { get; set; } // Nullable for anonymous views
    public User? User { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public int ViewDurationSeconds { get; set; } = 0; // How long user viewed the post
}

public class SEOSettings : BaseEntity
{
    public string PageType { get; set; } = string.Empty; // Product, Category, Blog, Page, Home
    public Guid? EntityId { get; set; } // ID of the entity (ProductId, CategoryId, etc.)
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? CanonicalUrl { get; set; }
    public string? OgTitle { get; set; } // Open Graph title
    public string? OgDescription { get; set; } // Open Graph description
    public string? OgImageUrl { get; set; } // Open Graph image
    public string? TwitterCard { get; set; } // summary, summary_large_image
    public string? StructuredData { get; set; } // JSON-LD structured data
    public bool IsIndexed { get; set; } = true; // Allow search engines to index
    public bool FollowLinks { get; set; } = true; // Follow or nofollow
    public decimal Priority { get; set; } = 0.5m; // Sitemap priority (0.0 to 1.0)
    public string? ChangeFrequency { get; set; } // always, hourly, daily, weekly, monthly, yearly, never
}

public class SitemapEntry : BaseEntity
{
    public string Url { get; set; } = string.Empty;
    public string PageType { get; set; } = string.Empty; // Product, Category, Blog, Page
    public Guid? EntityId { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public string ChangeFrequency { get; set; } = "weekly"; // always, hourly, daily, weekly, monthly, yearly, never
    public decimal Priority { get; set; } = 0.5m; // 0.0 to 1.0
    public bool IsActive { get; set; } = true;
}

