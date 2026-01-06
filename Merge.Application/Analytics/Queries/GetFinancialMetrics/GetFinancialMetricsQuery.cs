using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetFinancialMetrics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetFinancialMetricsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<FinancialMetricsDto>;

