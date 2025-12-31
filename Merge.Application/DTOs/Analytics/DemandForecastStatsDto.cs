namespace Merge.Application.DTOs.Analytics;

public class DemandForecastStatsDto
{
    public int TotalProducts { get; set; }
    public int ProductsWithSales { get; set; }
    public decimal ForecastCoverage { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
