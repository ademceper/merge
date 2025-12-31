namespace Merge.Application.DTOs.Analytics;

public class PriceRecommendationDto
{
    public decimal OptimalPrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal Confidence { get; set; }
    public decimal ExpectedRevenueChange { get; set; }
    public int ExpectedSalesChange { get; set; }
    public string Reasoning { get; set; } = string.Empty;
}
