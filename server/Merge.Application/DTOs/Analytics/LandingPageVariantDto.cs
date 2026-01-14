namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record LandingPageVariantDto(
    Guid Id,
    string Name,
    int Views,
    int Conversions,
    decimal ConversionRate,
    int TrafficSplit
);
