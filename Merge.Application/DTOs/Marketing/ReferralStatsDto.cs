namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Referral Stats DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record ReferralStatsDto(
    int TotalReferrals,
    int CompletedReferrals,
    int PendingReferrals,
    int TotalPointsAwarded,
    decimal ConversionRate);
