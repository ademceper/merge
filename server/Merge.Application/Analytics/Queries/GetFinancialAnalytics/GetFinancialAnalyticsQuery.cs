using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetFinancialAnalytics;

public record GetFinancialAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<FinancialAnalyticsDto>;

