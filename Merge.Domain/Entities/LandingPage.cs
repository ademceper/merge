using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// LandingPage Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LandingPage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // JSON or HTML content
    public string? Template { get; set; } // Template identifier
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public Guid? AuthorId { get; set; }
    public User? Author { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? StartDate { get; set; } // When to start showing
    public DateTime? EndDate { get; set; } // When to stop showing
    public bool IsActive { get; set; } = true;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public int ViewCount { get; set; } = 0;
    public int ConversionCount { get; set; } = 0; // Track conversions
    public decimal ConversionRate { get; set; } = 0; // Percentage
    public bool EnableABTesting { get; set; } = false;
    public Guid? VariantOfId { get; set; } // If this is a variant for A/B testing
    public LandingPage? VariantOf { get; set; }
    public ICollection<LandingPage> Variants { get; set; } = new List<LandingPage>();
    public int TrafficSplit { get; set; } = 50; // Percentage of traffic for A/B testing
}

