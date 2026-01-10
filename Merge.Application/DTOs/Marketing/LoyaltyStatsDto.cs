namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Loyalty Stats DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record LoyaltyStatsDto(
    int TotalMembers,
    long TotalPointsIssued,
    long TotalPointsRedeemed,
    Dictionary<string, int> MembersByTier,
    decimal AveragePointsPerMember);
