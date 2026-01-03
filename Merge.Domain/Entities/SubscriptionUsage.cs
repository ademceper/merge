namespace Merge.Domain.Entities;

/// <summary>
/// SubscriptionUsage Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SubscriptionUsage : BaseEntity
{
    public Guid UserSubscriptionId { get; set; }
    public string Feature { get; set; } = string.Empty; // Feature name being used
    public int UsageCount { get; set; } = 0;
    public int? Limit { get; set; } // Usage limit for this feature
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    
    // Navigation properties
    public UserSubscription UserSubscription { get; set; } = null!;
}

