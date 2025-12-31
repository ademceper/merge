namespace Merge.Application.DTOs.Seller;

public class SellerDashboardStatsDto
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TodayOrders { get; set; }
    public decimal TodayRevenue { get; set; }
    public int LowStockProducts { get; set; }
}
