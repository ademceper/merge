namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record PriceOptimizationDto(
    Guid ProductId,
    string ProductName,
    decimal CurrentPrice,
    decimal RecommendedPrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal ExpectedRevenueChange,
    int ExpectedSalesChange,
    decimal Confidence,
    string Reasoning,
    DateTime OptimizedAt
);
