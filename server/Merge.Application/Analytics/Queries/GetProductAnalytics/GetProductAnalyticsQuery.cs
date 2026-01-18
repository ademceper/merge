using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetProductAnalytics;

public record GetProductAnalyticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<ProductAnalyticsDto>;

