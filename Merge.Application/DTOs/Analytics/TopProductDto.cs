namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record TopProductDto(
    Guid ProductId,
    string ProductName,
    string SKU,
    int UnitsSold,
    decimal Revenue,
    decimal AveragePrice
);
