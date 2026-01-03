namespace Merge.Domain.Entities;

/// <summary>
/// LoyaltyTier Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LoyaltyTier : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Bronze, Silver, Gold, Platinum
    public string Description { get; set; } = string.Empty;
    public int MinimumPoints { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;
    public decimal PointsMultiplier { get; set; } = 1.0m;
    public string Benefits { get; set; } = string.Empty; // JSON or comma-separated
    public string Color { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public int Level { get; set; } // 1 = Bronze, 2 = Silver, etc.
    public bool IsActive { get; set; } = true;
}

