namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record DashboardStatsDto(
    int TotalUsers,
    int ActiveUsers,
    int TotalProducts,
    int ActiveProducts,
    int TotalOrders,
    decimal TotalRevenue,
    int PendingOrders,
    int TodayOrders,
    decimal TodayRevenue,
    int TotalWarehouses,
    int ActiveWarehouses,
    int LowStockProducts,
    int TotalCategories,
    int PendingReviews,
    int PendingReturns,
    int Users2FAEnabled
);
