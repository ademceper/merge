namespace Merge.Application.DTOs.Seller;

public class StoreStatsDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int TotalCustomers { get; set; }
    public decimal AverageRating { get; set; }
}
