using MediatR;
using Merge.Application.DTOs.Subscription;
using Merge.Domain.Enums;

namespace Merge.Application.Subscription.Commands.UpdateSubscriptionPlan;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateSubscriptionPlanCommand(
    Guid Id,
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    int? DurationDays = null,
    int? TrialDays = null,
    SubscriptionPlanFeaturesDto? Features = null,
    bool? IsActive = null,
    int? DisplayOrder = null,
    BillingCycle? BillingCycle = null,
    int? MaxUsers = null,
    decimal? SetupFee = null,
    string? Currency = null) : IRequest<bool>;
