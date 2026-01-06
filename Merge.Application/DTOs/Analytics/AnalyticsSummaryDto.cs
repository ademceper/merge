namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
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
