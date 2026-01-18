using MediatR;
using Merge.Application.DTOs.Subscription;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Subscription.Commands.CreateSubscriptionPlan;

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
