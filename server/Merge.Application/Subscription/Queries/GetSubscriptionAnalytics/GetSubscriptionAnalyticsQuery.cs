using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Queries.GetSubscriptionAnalytics;

public record GetSubscriptionAnalyticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<SubscriptionAnalyticsDto>;
