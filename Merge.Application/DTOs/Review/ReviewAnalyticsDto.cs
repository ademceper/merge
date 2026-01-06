namespace Merge.Application.DTOs.Review;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record ReviewAnalyticsDto(
    DateTime StartDate,
    DateTime EndDate,
    int TotalReviews,
    int ApprovedReviews,
    int PendingReviews,
    int RejectedReviews,
    decimal AverageRating,
    int ReviewsWithMedia,
    int VerifiedPurchaseReviews,
    decimal HelpfulPercentage,
    List<RatingDistributionDto> RatingDistribution,
    List<ReviewTrendDto> ReviewTrends,
    List<TopReviewedProductDto> TopReviewedProducts,
    List<ReviewerStatsDto> TopReviewers
);
