namespace Merge.Application.DTOs.Analytics;

public class MarketingAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalCampaigns { get; set; }
    public int ActiveCoupons { get; set; }
    public int CouponUsageCount { get; set; }
    public decimal TotalDiscountsGiven { get; set; }
    public decimal EmailMarketingROI { get; set; }
    public List<CouponPerformanceDto> TopCoupons { get; set; } = new();
    public List<ReferralPerformanceDto> ReferralStats { get; set; } = new();
}
