namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record CategorySalesDto(
    Guid CategoryId,
    string CategoryName,
    decimal Revenue,
    int OrderCount,
    int ProductsSold
);
