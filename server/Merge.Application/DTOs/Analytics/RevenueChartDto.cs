namespace Merge.Application.DTOs.Analytics;

public record RevenueChartDto(
    int Days,
    decimal TotalRevenue,
    int TotalOrders,
    List<DailyRevenueDto> DailyData
);
