using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Interfaces.ML;

public interface IPriceOptimizationService
{
    Task<PriceOptimizationDto> OptimizePriceAsync(Guid productId, PriceOptimizationRequestDto? request = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<PriceOptimizationDto>> OptimizePricesForCategoryAsync(Guid categoryId, PriceOptimizationRequestDto? request = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<PriceRecommendationDto>> GetPriceRecommendationsAsync(Guid productId, int count = 5, CancellationToken cancellationToken = default);
    Task<PriceRecommendationDto> GetPriceRecommendationAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<PriceOptimizationStatsDto> GetOptimizationStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

