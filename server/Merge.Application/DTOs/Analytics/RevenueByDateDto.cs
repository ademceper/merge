namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record RevenueByDateDto(
    DateTime Date,
    decimal Revenue,
    decimal Costs,
    decimal Profit,
    int OrderCount
);
