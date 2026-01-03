using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using PaymentStatus = Merge.Domain.Enums.PaymentStatus;

namespace Merge.Domain.Entities;

/// <summary>
/// UserSubscription Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class UserSubscription : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine)
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public bool AutoRenew { get; set; } = true;
    public DateTime? NextBillingDate { get; set; }
    public decimal CurrentPrice { get; set; } // Price at time of subscription
    public string? PaymentMethodId { get; set; } // Reference to payment method
    public int RenewalCount { get; set; } = 0; // How many times renewed

    // ✅ CONCURRENCY: Eşzamanlı güncellemeleri önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public ICollection<SubscriptionPayment> Payments { get; set; } = new List<SubscriptionPayment>();
}

