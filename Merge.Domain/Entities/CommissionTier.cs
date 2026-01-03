namespace Merge.Domain.Entities;

/// <summary>
/// CommissionTier Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class CommissionTier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal MinSales { get; set; } = 0; // Minimum sales to qualify for this tier
    public decimal MaxSales { get; set; } = decimal.MaxValue;
    public decimal CommissionRate { get; set; } // Percentage
    public decimal PlatformFeeRate { get; set; } = 0; // Percentage
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0; // Higher priority tiers checked first
}

