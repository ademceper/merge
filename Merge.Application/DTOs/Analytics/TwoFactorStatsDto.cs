namespace Merge.Application.DTOs.Analytics;

public class TwoFactorStatsDto
{
    public int TotalUsers { get; set; }
    public int UsersWithTwoFactor { get; set; }
    public decimal TwoFactorPercentage { get; set; }
    public List<TwoFactorMethodCount> MethodBreakdown { get; set; } = new();
}
