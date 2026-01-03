using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// LoyaltyRule Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LoyaltyRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LoyaltyTransactionType Type { get; set; }
    public int PointsAwarded { get; set; }
    public decimal? MinimumPurchaseAmount { get; set; }
    public int? ExpiryDays { get; set; } // Points expire after X days
    public bool IsActive { get; set; } = true;
}

