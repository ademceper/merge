namespace Merge.Application.DTOs.Analytics;

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
