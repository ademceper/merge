using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetRevenueChart;

public record GetRevenueChartQuery(
    int Days
) : IRequest<RevenueChartDto>;

