using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetDashboardSummary;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetDashboardSummaryQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<DashboardSummaryDto>;

