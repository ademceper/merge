namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
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
