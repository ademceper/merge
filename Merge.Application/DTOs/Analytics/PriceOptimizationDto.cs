namespace Merge.Application.DTOs.Analytics;

public class PriceOptimizationDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal RecommendedPrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal ExpectedRevenueChange { get; set; }
    public int ExpectedSalesChange { get; set; }
    public decimal Confidence { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public DateTime OptimizedAt { get; set; }
}
