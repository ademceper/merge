using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Queries.GetSubscriptionTrends;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSubscriptionTrendsQuery(
    DateTime StartDate,
    DateTime EndDate) : IRequest<IEnumerable<SubscriptionTrendDto>>;
