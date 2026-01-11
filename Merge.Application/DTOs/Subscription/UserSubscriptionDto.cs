using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Subscription;

public class UserSubscriptionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid SubscriptionPlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public bool IsTrial { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public bool AutoRenew { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public decimal CurrentPrice { get; set; }
    public int RenewalCount { get; set; }
    public int DaysRemaining { get; set; }
    public List<SubscriptionPaymentDto> RecentPayments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
