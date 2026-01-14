using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.ML.Queries.GetPriceOptimizationStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPriceOptimizationStatsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<PriceOptimizationStatsDto>;
