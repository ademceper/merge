namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record ReferralPerformanceDto(
    int TotalReferrals,
    int SuccessfulReferrals,
    decimal ConversionRate,
    decimal TotalRewardsGiven
);
