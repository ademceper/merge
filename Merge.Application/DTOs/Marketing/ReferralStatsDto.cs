namespace Merge.Application.DTOs.Marketing;

public class ReferralStatsDto
{
    public int TotalReferrals { get; set; }
    public int CompletedReferrals { get; set; }
    public int PendingReferrals { get; set; }
    public int TotalPointsAwarded { get; set; }
    public decimal ConversionRate { get; set; }
}
