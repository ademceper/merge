namespace Merge.Application.DTOs.Analytics;

public class RevenueChartDto
{
    public int Days { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public List<DailyRevenueDto> DailyData { get; set; } = new();
}
