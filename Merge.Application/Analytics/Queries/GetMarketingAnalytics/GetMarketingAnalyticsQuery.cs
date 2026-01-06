using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetMarketingAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetMarketingAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<MarketingAnalyticsDto>;

