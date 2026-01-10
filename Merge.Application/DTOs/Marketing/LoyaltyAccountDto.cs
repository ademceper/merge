namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Loyalty Account DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record LoyaltyAccountDto(
    Guid Id,
    Guid UserId,
    int PointsBalance,
    int LifetimePoints,
    string TierName,
    int TierLevel,
    DateTime? TierExpiresAt);
