namespace Merge.Domain.Entities;

/// <summary>
/// ProductSizeGuide Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ProductSizeGuide : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid SizeGuideId { get; set; }
    public SizeGuide SizeGuide { get; set; } = null!;
    public string? CustomNotes { get; set; } // Product-specific sizing notes
    public bool FitType { get; set; } = true; // true = Regular Fit, false = Slim Fit, etc.
    public string? FitDescription { get; set; }
}

