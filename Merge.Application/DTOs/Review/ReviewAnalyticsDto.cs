namespace Merge.Application.DTOs.Review;

public class ReviewAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalReviews { get; set; }
    public int ApprovedReviews { get; set; }
    public int PendingReviews { get; set; }
    public int RejectedReviews { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewsWithMedia { get; set; }
    public int VerifiedPurchaseReviews { get; set; }
    public decimal HelpfulPercentage { get; set; }
    public List<RatingDistributionDto> RatingDistribution { get; set; } = new();
    public List<ReviewTrendDto> ReviewTrends { get; set; } = new();
    public List<TopReviewedProductDto> TopReviewedProducts { get; set; } = new();
    public List<ReviewerStatsDto> TopReviewers { get; set; } = new();
}
