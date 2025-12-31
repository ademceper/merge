namespace Merge.Application.DTOs.Subscription;

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public int? TrialDays { get; set; }
    public Dictionary<string, object>? Features { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? BillingCycle { get; set; }
    public int MaxUsers { get; set; }
    public decimal? SetupFee { get; set; }
    public string? Currency { get; set; }
    public int SubscriberCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
