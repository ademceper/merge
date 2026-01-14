namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
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
