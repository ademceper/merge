using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// SubscriptionPlan Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
    public SubscriptionPlanType PlanType { get; set; } = SubscriptionPlanType.Monthly;
    public decimal Price { get; set; }
    public int DurationDays { get; set; } // How many days the subscription lasts
    public int? TrialDays { get; set; } // Free trial period
    public string? Features { get; set; } // JSON string for plan features
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public int MaxUsers { get; set; } = 1; // Maximum users allowed
    public decimal? SetupFee { get; set; } // One-time setup fee
    public string? Currency { get; set; } = "TRY";

    // Navigation properties
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}

