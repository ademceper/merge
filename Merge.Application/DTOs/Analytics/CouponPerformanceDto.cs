namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record CouponPerformanceDto(
    Guid CouponId,
    string Code,
    int UsageCount,
    decimal TotalDiscount,
    decimal RevenueGenerated
);
