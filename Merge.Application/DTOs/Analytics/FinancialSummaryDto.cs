namespace Merge.Application.DTOs.Analytics;

public class FinancialSummaryDto
{
    public DateTime Period { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCosts { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public int TotalOrders { get; set; }
}
