using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// KnowledgeBaseArticle Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class KnowledgeBaseArticle : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // URL-friendly identifier
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; } // Short summary
    public Guid? CategoryId { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
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

