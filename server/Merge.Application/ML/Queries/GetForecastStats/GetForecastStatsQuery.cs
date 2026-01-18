using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.ML.Queries.GetForecastStats;

public record GetForecastStatsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<DemandForecastStatsDto>;
