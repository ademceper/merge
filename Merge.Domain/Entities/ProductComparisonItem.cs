namespace Merge.Domain.Entities;

/// <summary>
/// ProductComparisonItem Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ProductComparisonItem : BaseEntity
{
    public Guid ComparisonId { get; set; }
    public ProductComparison Comparison { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Position { get; set; } = 0; // Order in comparison
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

