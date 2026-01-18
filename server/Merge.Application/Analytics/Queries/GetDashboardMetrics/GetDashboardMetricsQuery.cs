using MediatR;
using Merge.Application.DTOs.Analytics;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Analytics.Queries.GetDashboardMetrics;

public record GetDashboardMetricsQuery(
    string? Category = null
) : IRequest<List<DashboardMetricDto>>;

