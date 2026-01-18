namespace Merge.Application.DTOs.Analytics;

public record PriceOptimizationStatsDto(
    int TotalOptimizations,
    decimal AverageRevenueIncrease,
    int ProductsOptimized,
    DateTime? PeriodStart,
    DateTime? PeriodEnd
);
