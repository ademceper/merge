namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record TimeSeriesDataPoint(
    DateTime Date,
    decimal Value,
    string? Label = null,
    int? Count = null
);

