namespace Merge.Application.DTOs.Analytics;

public record PriceRecommendationDto(
    decimal OptimalPrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Confidence,
    decimal ExpectedRevenueChange,
    int ExpectedSalesChange,
    string Reasoning
);
