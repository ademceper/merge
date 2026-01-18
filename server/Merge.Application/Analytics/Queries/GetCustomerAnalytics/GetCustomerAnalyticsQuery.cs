using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetCustomerAnalytics;

public record GetCustomerAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<CustomerAnalyticsDto>;

