namespace Merge.Application.DTOs.Analytics;

public record ProductAnalyticsDto(
    DateTime StartDate,
    DateTime EndDate,
    int TotalProducts,
    int ActiveProducts,
    int OutOfStockProducts,
    int LowStockProducts,
    decimal TotalInventoryValue,
    List<TopProductDto> BestSellers,
    List<TopProductDto> WorstPerformers,
    List<ProductCategoryPerformanceDto> CategoryPerformance
);
