using Merge.Domain.Enums;
using PaymentStatus = Merge.Domain.Enums.PaymentStatus;

namespace Merge.Domain.Entities;

/// <summary>
/// SubscriptionPayment Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SubscriptionPayment : BaseEntity
{
    public Guid UserSubscriptionId { get; set; }
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; } // External payment gateway transaction ID
    public DateTime? PaidAt { get; set; }
    public DateTime BillingPeriodStart { get; set; }
    public DateTime BillingPeriodEnd { get; set; }
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime? NextRetryDate { get; set; }
    
    // Navigation properties
    public UserSubscription UserSubscription { get; set; } = null!;
}

