namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record RevenueChartDto(
    int Days,
    decimal TotalRevenue,
    int TotalOrders,
    List<DailyRevenueDto> DailyData
);
