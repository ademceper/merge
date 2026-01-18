using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetSalesAnalytics;

public record GetSalesAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<SalesAnalyticsDto>;

