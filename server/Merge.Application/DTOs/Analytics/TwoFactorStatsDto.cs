namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record TwoFactorStatsDto(
    int TotalUsers,
    int UsersWithTwoFactor,
    decimal TwoFactorPercentage,
    List<TwoFactorMethodCount> MethodBreakdown
);
