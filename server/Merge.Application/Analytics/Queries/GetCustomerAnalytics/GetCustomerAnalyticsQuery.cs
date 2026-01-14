using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetCustomerAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCustomerAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<CustomerAnalyticsDto>;

