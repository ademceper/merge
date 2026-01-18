namespace Merge.Application.DTOs.Analytics;

public record LandingPageVariantDto(
    Guid Id,
    string Name,
    int Views,
    int Conversions,
    decimal ConversionRate,
    int TrafficSplit
);
