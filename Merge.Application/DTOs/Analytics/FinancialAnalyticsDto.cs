namespace Merge.Application.DTOs.Analytics;

public class FinancialAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal TotalCosts { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalRefunds { get; set; }
    public decimal TotalShippingCosts { get; set; }
    public List<TimeSeriesDataPoint> RevenueTimeSeries { get; set; } = new();
    public List<TimeSeriesDataPoint> ProfitTimeSeries { get; set; } = new();
}
