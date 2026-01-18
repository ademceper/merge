using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Analytics;

public record FinancialReportDto(
    DateTime StartDate,
    DateTime EndDate,
    // Revenue
    decimal TotalRevenue,
    decimal ProductRevenue,
    decimal ShippingRevenue,
    decimal TaxCollected,
    // Costs
    decimal TotalCosts,
    decimal ProductCosts,
    decimal ShippingCosts,
    decimal PlatformFees,
    decimal CommissionPaid,
    decimal RefundAmount,
    decimal DiscountGiven,
    // Profit
    decimal GrossProfit,
    decimal NetProfit,
    decimal ProfitMargin, // Percentage
    // Breakdowns
    List<RevenueByCategoryDto> RevenueByCategory,
    List<RevenueByDateDto> RevenueByDate,
    List<ExpenseByTypeDto> ExpensesByType,
    // Trends
    decimal RevenueGrowth, // Percentage vs previous period
    decimal ProfitGrowth, // Percentage vs previous period
    decimal AverageOrderValue,
    int TotalOrders
);
