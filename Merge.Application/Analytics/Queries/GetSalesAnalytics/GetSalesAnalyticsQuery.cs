using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetSalesAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSalesAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<SalesAnalyticsDto>;

