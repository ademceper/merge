using MediatR;
using Merge.Application.DTOs.Subscription;
using Merge.Domain.Enums;

namespace Merge.Application.Subscription.Commands.CreateSubscriptionPlan;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateSubscriptionPlanCommand(
    string Name,
    string Description,
    SubscriptionPlanType PlanType,
    decimal Price,
    int DurationDays,
    BillingCycle BillingCycle,
    int MaxUsers = 1,
    int? TrialDays = null,
    decimal? SetupFee = null,
    string Currency = "TRY",
    SubscriptionPlanFeaturesDto? Features = null,
    bool IsActive = true,
    int DisplayOrder = 0) : IRequest<SubscriptionPlanDto>;
