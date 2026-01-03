using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// SizeGuide Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SizeGuide : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Men's Shirt Sizes", "Women's Shoe Sizes"
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string? Brand { get; set; } // Brand-specific size guide
    public SizeGuideType Type { get; set; } = SizeGuideType.Standard;
    public string MeasurementUnit { get; set; } = "cm"; // cm, inch, etc.
    public bool IsActive { get; set; } = true;
    public ICollection<SizeGuideEntry> Entries { get; set; } = new List<SizeGuideEntry>();
    public ICollection<ProductSizeGuide> ProductSizeGuides { get; set; } = new List<ProductSizeGuide>();
}

