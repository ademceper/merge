using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Queries.GetAllSubscriptionPlans;

public record GetAllSubscriptionPlansQuery(bool? IsActive = null) : IRequest<IEnumerable<SubscriptionPlanDto>>;
