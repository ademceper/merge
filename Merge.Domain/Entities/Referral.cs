using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// Referral Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Referral : BaseEntity
{
    public Guid ReferrerId { get; set; } // User who referred
    public Guid ReferredUserId { get; set; } // User who was referred
    public Guid ReferralCodeId { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public ReferralStatus Status { get; set; } = ReferralStatus.Pending;
    public DateTime? CompletedAt { get; set; }
    public int PointsAwarded { get; set; } = 0;
    public Guid? FirstOrderId { get; set; } // First order of referred user

    // Navigation properties
    public User Referrer { get; set; } = null!;
    public User ReferredUser { get; set; } = null!;
    public ReferralCode ReferralCodeEntity { get; set; } = null!;
    public Order? FirstOrder { get; set; }
}

