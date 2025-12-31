namespace Merge.Application.DTOs.Analytics;

public class DemandForecastDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ForecastDays { get; set; }
    public int ForecastedQuantity { get; set; }
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public decimal Confidence { get; set; }
    public List<DailyForecastItem> DailyForecast { get; set; } = new();
    public string Reasoning { get; set; } = string.Empty;
    public DateTime ForecastedAt { get; set; }
}
