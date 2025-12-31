namespace Merge.Application.DTOs.Review;

public class ReviewerStatsDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ReviewCount { get; set; }
    public decimal AverageRating { get; set; }
    public int HelpfulVotes { get; set; }
}
