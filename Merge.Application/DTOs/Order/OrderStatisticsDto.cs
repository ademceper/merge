namespace Merge.Application.DTOs.Order;

public class OrderStatisticsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public Dictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = new Dictionary<string, decimal>();
}
