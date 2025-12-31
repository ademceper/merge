namespace Merge.Application.DTOs.Analytics;

public class RevenueByCategoryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public decimal Percentage { get; set; }
}
