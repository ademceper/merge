namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record FinancialSummaryDto(
    DateTime Period,
    decimal TotalRevenue,
    decimal TotalCosts,
    decimal NetProfit,
    decimal ProfitMargin,
    int TotalOrders
);
