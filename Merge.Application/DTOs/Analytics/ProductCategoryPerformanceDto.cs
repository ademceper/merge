namespace Merge.Application.DTOs.Analytics;

public class ProductCategoryPerformanceDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalStock { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal TotalValue { get; set; }
}
