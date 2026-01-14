using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetRevenueOverTime;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetRevenueOverTimeQuery(
    DateTime StartDate,
    DateTime EndDate,
    string Interval
) : IRequest<List<TimeSeriesDataPoint>>;

