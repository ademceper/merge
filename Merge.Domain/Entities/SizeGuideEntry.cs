namespace Merge.Domain.Entities;

/// <summary>
/// SizeGuideEntry Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SizeGuideEntry : BaseEntity
{
    public Guid SizeGuideId { get; set; }
    public SizeGuide SizeGuide { get; set; } = null!;
    public string SizeLabel { get; set; } = string.Empty; // XS, S, M, L, XL, 38, 40, etc.
    public string? AlternativeLabel { get; set; } // US 8, EU 38, UK 10
    public decimal? Chest { get; set; }
    public decimal? Waist { get; set; }
    public decimal? Hips { get; set; }
    public decimal? Inseam { get; set; }
    public decimal? Shoulder { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; } // For height-based sizing
    public decimal? Weight { get; set; } // For weight-based sizing
    public string? AdditionalMeasurements { get; set; } // JSON for custom measurements
    public int DisplayOrder { get; set; } = 0;
}

