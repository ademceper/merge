namespace Merge.Application.DTOs.Analytics;

public class PriceOptimizationStatsDto
{
    public int TotalOptimizations { get; set; }
    public decimal AverageRevenueIncrease { get; set; }
    public int ProductsOptimized { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
}
