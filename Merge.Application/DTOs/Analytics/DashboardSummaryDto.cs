namespace Merge.Application.DTOs.Analytics;

public class DashboardSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueChange { get; set; }
    public int TotalOrders { get; set; }
    public decimal OrdersChange { get; set; }
    public int TotalCustomers { get; set; }
    public decimal CustomersChange { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal AOVChange { get; set; }
    public int PendingOrders { get; set; }
    public int LowStockProducts { get; set; }
    public List<DashboardMetricDto> Metrics { get; set; } = new();
}
