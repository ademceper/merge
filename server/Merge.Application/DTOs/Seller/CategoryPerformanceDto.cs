namespace Merge.Application.DTOs.Seller;

public record CategoryPerformanceDto
{
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int ProductCount { get; init; }
    public int OrderCount { get; init; }
    public int OrdersCount { get; init; } // Alias for OrderCount
    public decimal TotalSales { get; init; }
    public decimal Revenue { get; init; }
    public decimal AverageRating { get; init; }
}
