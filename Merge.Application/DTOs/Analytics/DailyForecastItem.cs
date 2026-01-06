namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record DailyForecastItem(
    DateTime Date,
    int Quantity
);

