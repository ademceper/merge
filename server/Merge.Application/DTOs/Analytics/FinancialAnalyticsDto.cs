namespace Merge.Application.DTOs.Analytics;

public record FinancialAnalyticsDto(
    DateTime StartDate,
    DateTime EndDate,
    decimal GrossRevenue,
    decimal TotalCosts,
    decimal NetProfit,
    decimal ProfitMargin,
    decimal TotalTax,
    decimal TotalRefunds,
    decimal TotalShippingCosts,
    List<TimeSeriesDataPoint> RevenueTimeSeries,
    List<TimeSeriesDataPoint> ProfitTimeSeries
);
