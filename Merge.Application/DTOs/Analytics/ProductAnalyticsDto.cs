namespace Merge.Application.DTOs.Analytics;

public class ProductAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int LowStockProducts { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public List<TopProductDto> BestSellers { get; set; } = new();
    public List<TopProductDto> WorstPerformers { get; set; } = new();
    public List<ProductCategoryPerformanceDto> CategoryPerformance { get; set; } = new();
}
