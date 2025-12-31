namespace Merge.Domain.Entities;

public class KnowledgeBaseArticle : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // URL-friendly identifier
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; } // Short summary
    public Guid? CategoryId { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Published, Archived
    public int ViewCount { get; set; } = 0;
    public int HelpfulCount { get; set; } = 0;
    public int NotHelpfulCount { get; set; } = 0;
    public bool IsFeatured { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public string? Tags { get; set; } // Comma-separated tags
    public Guid? AuthorId { get; set; } // Admin/Staff who created it
    public DateTime? PublishedAt { get; set; }
    
    // Navigation properties
    public KnowledgeBaseCategory? Category { get; set; }
    public User? Author { get; set; }
    public ICollection<KnowledgeBaseView> Views { get; set; } = new List<KnowledgeBaseView>();
}

public class KnowledgeBaseCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? IconUrl { get; set; }
    
    // Navigation properties
    public KnowledgeBaseCategory? ParentCategory { get; set; }
    public ICollection<KnowledgeBaseCategory> SubCategories { get; set; } = new List<KnowledgeBaseCategory>();
    public ICollection<KnowledgeBaseArticle> Articles { get; set; } = new List<KnowledgeBaseArticle>();
}

public class KnowledgeBaseView : BaseEntity
{
    public Guid ArticleId { get; set; }
    public Guid? UserId { get; set; } // Nullable for anonymous views
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public int ViewDuration { get; set; } = 0; // Seconds
    
    // Navigation properties
    public KnowledgeBaseArticle Article { get; set; } = null!;
    public User? User { get; set; }
}

