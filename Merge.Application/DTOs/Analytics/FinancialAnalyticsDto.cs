namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
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
