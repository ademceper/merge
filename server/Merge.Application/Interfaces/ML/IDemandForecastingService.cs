using Merge.Application.DTOs.Analytics;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.ML;

public interface IDemandForecastingService
{
    Task<DemandForecastDto> ForecastDemandAsync(Guid productId, int forecastDays = 30, CancellationToken cancellationToken = default);
    Task<IEnumerable<DemandForecastDto>> ForecastDemandForCategoryAsync(Guid categoryId, int forecastDays = 30, CancellationToken cancellationToken = default);
    Task<DemandForecastStatsDto> GetForecastStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

