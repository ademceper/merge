using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Queries.GetAllSubscriptionPlans;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllSubscriptionPlansQuery(bool? IsActive = null) : IRequest<IEnumerable<SubscriptionPlanDto>>;
