using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Queries.GetSubscriptionAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSubscriptionAnalyticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<SubscriptionAnalyticsDto>;
