namespace Merge.Domain.Entities;

/// <summary>
/// ReferralCode Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ReferralCode : BaseEntity
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int UsageCount { get; set; } = 0;
    public int MaxUsage { get; set; } = 0; // 0 = unlimited
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int PointsReward { get; set; } = 100;
    public decimal DiscountPercentage { get; set; } = 10;

    // Navigation properties
    public User User { get; set; } = null!;
}

