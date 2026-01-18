using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetMarketingAnalytics;

public record GetMarketingAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<MarketingAnalyticsDto>;

