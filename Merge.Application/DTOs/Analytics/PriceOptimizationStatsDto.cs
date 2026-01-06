namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record PriceOptimizationStatsDto(
    int TotalOptimizations,
    decimal AverageRevenueIncrease,
    int ProductsOptimized,
    DateTime? PeriodStart,
    DateTime? PeriodEnd
);
