namespace Merge.Application.DTOs.Analytics;

public record ReferralPerformanceDto(
    int TotalReferrals,
    int SuccessfulReferrals,
    decimal ConversionRate,
    decimal TotalRewardsGiven
);
