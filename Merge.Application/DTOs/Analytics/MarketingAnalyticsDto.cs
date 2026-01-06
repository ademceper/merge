namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record MarketingAnalyticsDto(
    DateTime StartDate,
    DateTime EndDate,
    int TotalCampaigns,
    int ActiveCoupons,
    int CouponUsageCount,
    decimal TotalDiscountsGiven,
    decimal EmailMarketingROI,
    List<CouponPerformanceDto> TopCoupons,
    List<ReferralPerformanceDto> ReferralStats
);
