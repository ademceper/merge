namespace Merge.Application.DTOs.Analytics;

public class DashboardMetricDto
{
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string ValueFormatted { get; set; } = string.Empty;
    public decimal? ChangePercentage { get; set; }
    public DateTime CalculatedAt { get; set; }
}
