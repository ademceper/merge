namespace Merge.Application.DTOs.Analytics;

public class RevenueByDateDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public decimal Costs { get; set; }
    public decimal Profit { get; set; }
    public int OrderCount { get; set; }
}
