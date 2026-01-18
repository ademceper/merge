namespace Merge.Application.DTOs.Analytics;

public record DemandForecastStatsDto(
    int TotalProducts,
    int ProductsWithSales,
    decimal ForecastCoverage,
    DateTime PeriodStart,
    DateTime PeriodEnd
);
