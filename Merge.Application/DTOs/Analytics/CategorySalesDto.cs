namespace Merge.Application.DTOs.Analytics;

public class CategorySalesDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public int ProductsSold { get; set; }
}
