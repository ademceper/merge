namespace Merge.Application.DTOs.Seller;

public record SellerDashboardStatsDto
{
    public int TotalProducts { get; init; }
    public int ActiveProducts { get; init; }
    public int TotalOrders { get; init; }
    public int PendingOrders { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal PendingBalance { get; init; }
    public decimal AvailableBalance { get; init; }
    public decimal AverageRating { get; init; }
    public int TotalReviews { get; init; }
    public int TodayOrders { get; init; }
    public decimal TodayRevenue { get; init; }
    public int LowStockProducts { get; init; }
}
