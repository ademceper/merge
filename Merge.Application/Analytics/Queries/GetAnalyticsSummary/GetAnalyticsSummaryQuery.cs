using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetAnalyticsSummary;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAnalyticsSummaryQuery(
    int Days
) : IRequest<AnalyticsSummaryDto>;

