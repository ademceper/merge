using Merge.Domain.Modules.Marketing;
namespace Merge.Application.DTOs.Marketing;


public record ReferralStatsDto(
    int TotalReferrals,
    int CompletedReferrals,
    int PendingReferrals,
    int TotalPointsAwarded,
    decimal ConversionRate);
