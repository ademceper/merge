namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record RevenueByCategoryDto(
    Guid CategoryId,
    string CategoryName,
    decimal Revenue,
    int OrderCount,
    decimal Percentage
);
