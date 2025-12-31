namespace Merge.Application.DTOs.Analytics;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int TodayOrders { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TotalWarehouses { get; set; }
    public int ActiveWarehouses { get; set; }
    public int LowStockProducts { get; set; }
    public int TotalCategories { get; set; }
    public int PendingReviews { get; set; }
    public int PendingReturns { get; set; }
    public int Users2FAEnabled { get; set; }
}
