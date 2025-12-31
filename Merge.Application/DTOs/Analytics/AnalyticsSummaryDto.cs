namespace Merge.Application.DTOs.Analytics;

public class AnalyticsSummaryDto
{
    public string Period { get; set; } = string.Empty;
    public int NewUsers { get; set; }
    public int NewOrders { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int NewProducts { get; set; }
    public int TotalReviews { get; set; }
    public decimal AverageRating { get; set; }
}
