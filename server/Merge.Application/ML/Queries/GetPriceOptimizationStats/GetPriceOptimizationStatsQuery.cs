using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.ML.Queries.GetPriceOptimizationStats;

public record GetPriceOptimizationStatsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<PriceOptimizationStatsDto>;
