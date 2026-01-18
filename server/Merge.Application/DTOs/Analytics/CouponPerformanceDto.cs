namespace Merge.Application.DTOs.Analytics;

public record CouponPerformanceDto(
    Guid CouponId,
    string Code,
    int UsageCount,
    decimal TotalDiscount,
    decimal RevenueGenerated
);
