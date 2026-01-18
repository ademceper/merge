using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetAnalyticsSummary;

public record GetAnalyticsSummaryQuery(
    int Days
) : IRequest<AnalyticsSummaryDto>;

