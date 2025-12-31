namespace Merge.Application.DTOs.Analytics;

public class CustomerSegmentDto
{
    public string Segment { get; set; } = string.Empty; // New, Active, Inactive, VIP
    public int CustomerCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
}
