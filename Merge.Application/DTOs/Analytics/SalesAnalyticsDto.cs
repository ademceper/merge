namespace Merge.Application.DTOs.Analytics;

public class SalesAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalShipping { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal NetRevenue { get; set; }
    public List<TimeSeriesDataPoint> RevenueOverTime { get; set; } = new();
    public List<TimeSeriesDataPoint> OrdersOverTime { get; set; } = new();
    public List<CategorySalesDto> SalesByCategory { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
}
