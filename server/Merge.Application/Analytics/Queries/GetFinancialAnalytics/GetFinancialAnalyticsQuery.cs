using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetFinancialAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetFinancialAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<FinancialAnalyticsDto>;

