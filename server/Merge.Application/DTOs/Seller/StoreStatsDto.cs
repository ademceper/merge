namespace Merge.Application.DTOs.Seller;

public record StoreStatsDto
{
    public Guid StoreId { get; init; }
    public string StoreName { get; init; } = string.Empty;
    public int TotalProducts { get; init; }
    public int ActiveProducts { get; init; }
    public int TotalOrders { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal MonthlyRevenue { get; init; }
    public int TotalCustomers { get; init; }
    public decimal AverageRating { get; init; }
}
