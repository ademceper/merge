using Merge.Domain.Modules.Marketing;
namespace Merge.Application.DTOs.Marketing;


public record ReferralCodeDto(
    Guid Id,
    string Code,
    int UsageCount,
    int MaxUsage,
    DateTime? ExpiresAt,
    bool IsActive,
    int PointsReward,
    decimal DiscountPercentage);
