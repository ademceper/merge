namespace Merge.Application.DTOs.Analytics;

public class TimeSeriesDataPoint
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public string? Label { get; set; }
    public int? Count { get; set; }
}

