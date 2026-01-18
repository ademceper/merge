using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetDashboardSummary;

public record GetDashboardSummaryQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<DashboardSummaryDto>;

