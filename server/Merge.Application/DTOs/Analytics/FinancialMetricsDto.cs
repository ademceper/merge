namespace Merge.Application.DTOs.Analytics;

/// <summary>
/// Financial Metrics DTO - BOLUM 4.3: Over-Posting Korumasi (ZORUNLU)
/// Dictionary<string, decimal> yerine typed DTO kullanılıyor
/// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
/// </summary>
public record FinancialMetricsDto(
    decimal TotalRevenue,
    decimal TotalCosts,
    decimal NetProfit,
    decimal ProfitMargin,
    decimal AverageOrderValue,
    int TotalOrders
);

