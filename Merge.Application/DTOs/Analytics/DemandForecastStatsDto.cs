namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record DemandForecastStatsDto(
    int TotalProducts,
    int ProductsWithSales,
    decimal ForecastCoverage,
    DateTime PeriodStart,
    DateTime PeriodEnd
);
