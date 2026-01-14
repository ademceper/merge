namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record AdminTopProductDto(
    Guid ProductId,
    string ProductName,
    string ImageUrl,
    int TotalSold,
    decimal TotalRevenue
);
