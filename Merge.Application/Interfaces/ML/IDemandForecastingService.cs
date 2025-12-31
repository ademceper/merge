using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Interfaces.ML;

public interface IDemandForecastingService
{
    Task<DemandForecastDto> ForecastDemandAsync(Guid productId, int forecastDays = 30);
    Task<IEnumerable<DemandForecastDto>> ForecastDemandForCategoryAsync(Guid categoryId, int forecastDays = 30);
    Task<DemandForecastStatsDto> GetForecastStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

