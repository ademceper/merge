using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Subscription;

/// <summary>
/// Partial update DTO for Subscription Plan (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchSubscriptionPlanDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public SubscriptionPlanType? PlanType { get; init; }
    public decimal? Price { get; init; }
    public int? DurationDays { get; init; }
    public int? TrialDays { get; init; }
    public SubscriptionPlanFeaturesDto? Features { get; init; }
    public bool? IsActive { get; init; }
    public int? DisplayOrder { get; init; }
    public BillingCycle? BillingCycle { get; init; }
    public int? MaxUsers { get; init; }
    public decimal? SetupFee { get; init; }
    public string? Currency { get; init; }
}
