namespace Merge.Application.DTOs.Review;

public class ReviewTrendDto
{
    public DateTime Date { get; set; }
    public int ReviewCount { get; set; }
    public decimal AverageRating { get; set; }
}
