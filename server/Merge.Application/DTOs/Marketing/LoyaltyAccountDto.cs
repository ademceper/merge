namespace Merge.Application.DTOs.Marketing;


public record LoyaltyAccountDto(
    Guid Id,
    Guid UserId,
    int PointsBalance,
    int LifetimePoints,
    string TierName,
    int TierLevel,
    DateTime? TierExpiresAt);
