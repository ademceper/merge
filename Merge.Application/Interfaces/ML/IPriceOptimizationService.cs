using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Interfaces.ML;

public interface IPriceOptimizationService
{
    Task<PriceOptimizationDto> OptimizePriceAsync(Guid productId, PriceOptimizationRequestDto? request = null);
    Task<IEnumerable<PriceOptimizationDto>> OptimizePricesForCategoryAsync(Guid categoryId, PriceOptimizationRequestDto? request = null);
    Task<IEnumerable<PriceRecommendationDto>> GetPriceRecommendationsAsync(Guid productId, int count = 5);
    Task<PriceRecommendationDto> GetPriceRecommendationAsync(Guid productId);
    Task<PriceOptimizationStatsDto> GetOptimizationStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

