namespace Merge.Domain.Entities;

/// <summary>
/// SEOSettings Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
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

