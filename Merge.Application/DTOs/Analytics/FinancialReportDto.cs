namespace Merge.Application.DTOs.Analytics;

public class FinancialReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Revenue
    public decimal TotalRevenue { get; set; }
    public decimal ProductRevenue { get; set; }
    public decimal ShippingRevenue { get; set; }
    public decimal TaxCollected { get; set; }
    
    // Costs
    public decimal TotalCosts { get; set; }
    public decimal ProductCosts { get; set; }
    public decimal ShippingCosts { get; set; }
    public decimal PlatformFees { get; set; }
    public decimal CommissionPaid { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal DiscountGiven { get; set; }
    
    // Profit
    public decimal GrossProfit { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; } // Percentage
    
    // Breakdowns
    public List<RevenueByCategoryDto> RevenueByCategory { get; set; } = new();
    public List<RevenueByDateDto> RevenueByDate { get; set; } = new();
    public List<ExpenseByTypeDto> ExpensesByType { get; set; } = new();
    
    // Trends
    public decimal RevenueGrowth { get; set; } // Percentage vs previous period
    public decimal ProfitGrowth { get; set; } // Percentage vs previous period
    public decimal AverageOrderValue { get; set; }
    public int TotalOrders { get; set; }
}
