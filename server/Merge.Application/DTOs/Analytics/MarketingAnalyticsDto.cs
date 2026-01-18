namespace Merge.Application.DTOs.Analytics;

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
