namespace Merge.Application.DTOs.Analytics;

public record DemandForecastDto(
    Guid ProductId,
    string ProductName,
    int ForecastDays,
    int ForecastedQuantity,
    int MinQuantity,
    int MaxQuantity,
    decimal Confidence,
    List<DailyForecastItem> DailyForecast,
    string Reasoning,
    DateTime ForecastedAt
);
