namespace Merge.Application.DTOs.Analytics;

public class CouponPerformanceDto
{
    public Guid CouponId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal RevenueGenerated { get; set; }
}
