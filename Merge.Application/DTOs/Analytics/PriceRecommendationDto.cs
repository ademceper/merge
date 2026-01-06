namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record PriceRecommendationDto(
    decimal OptimalPrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal Confidence,
    decimal ExpectedRevenueChange,
    int ExpectedSalesChange,
    string Reasoning
);
