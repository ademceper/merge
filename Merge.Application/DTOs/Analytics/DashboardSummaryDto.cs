namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record DashboardSummaryDto(
    decimal TotalRevenue,
    decimal RevenueChange,
    int TotalOrders,
    decimal OrdersChange,
    int TotalCustomers,
    decimal CustomersChange,
    decimal AverageOrderValue,
    decimal AOVChange,
    int PendingOrders,
    int LowStockProducts,
    List<DashboardMetricDto> Metrics
);
