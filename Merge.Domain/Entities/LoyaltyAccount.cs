using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Entities;

/// <summary>
/// LoyaltyAccount Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LoyaltyAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public int PointsBalance { get; set; } = 0;
    public int LifetimePoints { get; set; } = 0;
    public Guid? TierId { get; set; }
    public DateTime? TierAchievedAt { get; set; }
    public DateTime? TierExpiresAt { get; set; }

    // ✅ CONCURRENCY: RowVersion for optimistic concurrency control (puan işlemleri için kritik)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public LoyaltyTier? Tier { get; set; }
}

