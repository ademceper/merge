namespace Merge.Application.DTOs.Analytics;

public record RevenueByDateDto(
    DateTime Date,
    decimal Revenue,
    decimal Costs,
    decimal Profit,
    int OrderCount
);
