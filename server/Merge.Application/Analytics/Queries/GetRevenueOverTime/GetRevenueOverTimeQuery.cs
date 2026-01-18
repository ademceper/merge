using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetRevenueOverTime;

public record GetRevenueOverTimeQuery(
    DateTime StartDate,
    DateTime EndDate,
    string Interval
) : IRequest<List<TimeSeriesDataPoint>>;

