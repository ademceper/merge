namespace Merge.Application.DTOs.Analytics;

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
