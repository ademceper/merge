using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetRevenueChart;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetRevenueChartQuery(
    int Days
) : IRequest<RevenueChartDto>;

