namespace Merge.Application.DTOs.Analytics;

public record FinancialSummaryDto(
    DateTime Period,
    decimal TotalRevenue,
    decimal TotalCosts,
    decimal NetProfit,
    decimal ProfitMargin,
    int TotalOrders
);
