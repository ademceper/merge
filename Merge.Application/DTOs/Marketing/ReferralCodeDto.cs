using Merge.Domain.Modules.Marketing;
namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Referral Code DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record ReferralCodeDto(
    Guid Id,
    string Code,
    int UsageCount,
    int MaxUsage,
    DateTime? ExpiresAt,
    bool IsActive,
    int PointsReward,
    decimal DiscountPercentage);
