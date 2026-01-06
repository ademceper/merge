namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record DailyRevenueDto(
    DateTime Date,
    decimal Revenue,
    int OrderCount
);
