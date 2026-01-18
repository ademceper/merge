namespace Merge.Application.DTOs.Analytics;

public record DailyRevenueDto(
    DateTime Date,
    decimal Revenue,
    int OrderCount
);
