namespace Merge.Domain.Entities;

/// <summary>
/// TaxRule Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TaxRule : BaseEntity
{
    public string Country { get; set; } = string.Empty;
    public string? State { get; set; } // For countries with state-level tax
    public string? City { get; set; } // For cities with local tax
    public string TaxType { get; set; } = string.Empty; // VAT, GST, Sales Tax, etc.
    public decimal TaxRate { get; set; } // Percentage (e.g., 20 for 20%)
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? ProductCategoryIds { get; set; } // Comma separated category IDs (null = all categories)
    public bool IsInclusive { get; set; } = false; // Tax included in price or added on top
    public string? Notes { get; set; }
}

