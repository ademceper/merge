using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.ML.Queries.GetForecastStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetForecastStatsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<DemandForecastStatsDto>;
