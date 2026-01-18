using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.DTOs.Subscription;

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SubscriptionPlanType PlanType { get; set; }
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public int? TrialDays { get; set; }
    /// Typed DTO (Over-posting korumasi)
    public SubscriptionPlanFeaturesDto? Features { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public int MaxUsers { get; set; }
    public decimal? SetupFee { get; set; }
    public string? Currency { get; set; }
    public int SubscriberCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
