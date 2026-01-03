namespace Merge.Application.DTOs.Analytics;

/// <summary>
/// Financial Metrics DTO - BOLUM 4.3: Over-Posting Korumasi (ZORUNLU)
/// Dictionary<string, decimal> yerine typed DTO kullanılıyor
/// </summary>
public class FinancialMetricsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalCosts { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalOrders { get; set; }
}

