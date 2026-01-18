using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Queries.GetSubscriptionTrends;

public record GetSubscriptionTrendsQuery(
    DateTime StartDate,
    DateTime EndDate) : IRequest<IEnumerable<SubscriptionTrendDto>>;
