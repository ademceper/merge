namespace Merge.Application.DTOs.Analytics;

public record TimeSeriesDataPoint(
    DateTime Date,
    decimal Value,
    string? Label = null,
    int? Count = null
);

