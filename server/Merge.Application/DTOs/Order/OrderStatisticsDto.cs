using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Order;

public class OrderStatisticsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public Dictionary<string, int> OrdersByStatus { get; set; } = [];
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = [];
}
