namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record ProductCategoryPerformanceDto(
    Guid CategoryId,
    string CategoryName,
    int ProductCount,
    int TotalStock,
    decimal AveragePrice,
    decimal TotalValue
);
