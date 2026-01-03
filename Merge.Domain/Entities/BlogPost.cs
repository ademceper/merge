using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// BlogPost Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
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

