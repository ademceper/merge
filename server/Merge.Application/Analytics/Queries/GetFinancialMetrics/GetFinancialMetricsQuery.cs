using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetFinancialMetrics;

public record GetFinancialMetricsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<FinancialMetricsDto>;

