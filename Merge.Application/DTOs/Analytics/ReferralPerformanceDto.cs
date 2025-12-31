namespace Merge.Application.DTOs.Analytics;

public class ReferralPerformanceDto
{
    public int TotalReferrals { get; set; }
    public int SuccessfulReferrals { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal TotalRewardsGiven { get; set; }
}
