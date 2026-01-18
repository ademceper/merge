namespace Merge.Application.DTOs.Analytics;

public record AnalyticsSummaryDto(
    string Period,
    int NewUsers,
    int NewOrders,
    decimal Revenue,
    decimal AverageOrderValue,
    int NewProducts,
    int TotalReviews,
    decimal AverageRating
);
