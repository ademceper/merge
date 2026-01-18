namespace Merge.Application.DTOs.Analytics;

public record SalesAnalyticsDto(
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalRevenue,
    int TotalOrders,
    decimal AverageOrderValue,
    decimal TotalTax,
    decimal TotalShipping,
    decimal TotalDiscounts,
    decimal NetRevenue,
    List<TimeSeriesDataPoint> RevenueOverTime,
    List<TimeSeriesDataPoint> OrdersOverTime,
    List<CategorySalesDto> SalesByCategory,
    List<TopProductDto> TopProducts
);
