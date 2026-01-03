namespace Merge.Domain.Entities;

/// <summary>
/// ProductComparison Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ProductComparison : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = string.Empty; // Optional name for saved comparison
    public bool IsSaved { get; set; } = false;
    public string? ShareCode { get; set; } // For sharing comparisons
    public ICollection<ProductComparisonItem> Items { get; set; } = new List<ProductComparisonItem>();
}

