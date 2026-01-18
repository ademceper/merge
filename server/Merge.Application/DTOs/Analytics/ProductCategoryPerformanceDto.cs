namespace Merge.Application.DTOs.Analytics;

public record ProductCategoryPerformanceDto(
    Guid CategoryId,
    string CategoryName,
    int ProductCount,
    int TotalStock,
    decimal AveragePrice,
    decimal TotalValue
);
