namespace Merge.Application.DTOs.Marketing;


public record LoyaltyStatsDto(
    int TotalMembers,
    long TotalPointsIssued,
    long TotalPointsRedeemed,
    Dictionary<string, int> MembersByTier,
    decimal AveragePointsPerMember);
