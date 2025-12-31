namespace Merge.Application.DTOs.Review;

public class TopReviewedProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ReviewCount { get; set; }
    public decimal AverageRating { get; set; }
    public int HelpfulCount { get; set; }
}
