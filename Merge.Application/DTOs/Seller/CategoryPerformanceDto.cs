namespace Merge.Application.DTOs.Seller;

public class CategoryPerformanceDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public int OrdersCount { get; set; } // Alias for OrderCount
    public decimal TotalSales { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageRating { get; set; }
}
