namespace Merge.Application.DTOs.Analytics;

public record TwoFactorStatsDto(
    int TotalUsers,
    int UsersWithTwoFactor,
    decimal TwoFactorPercentage,
    List<TwoFactorMethodCount> MethodBreakdown
);
