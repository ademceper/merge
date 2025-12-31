namespace Merge.Application.DTOs.Seller;

public class CommissionStatsDto
{
    public int TotalCommissions { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal PendingCommissions { get; set; }
    public decimal ApprovedCommissions { get; set; }
    public decimal PaidCommissions { get; set; }
    public decimal AvailableForPayout { get; set; }
    public decimal AverageCommissionRate { get; set; }
    public decimal TotalPlatformFees { get; set; }
    public Dictionary<string, decimal> CommissionsByMonth { get; set; } = new();
}
