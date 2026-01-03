using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// PageBuilder Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PageBuilder : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // JSON content for page builder
    public string? Template { get; set; } // Template identifier
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public Guid? AuthorId { get; set; }
    public User? Author { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public int ViewCount { get; set; } = 0;
    public string? PageType { get; set; } // Home, Category, Product, Custom
    public Guid? RelatedEntityId { get; set; } // Related category/product ID if applicable
}

