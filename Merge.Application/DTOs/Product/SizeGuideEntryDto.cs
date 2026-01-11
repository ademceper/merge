namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record SizeGuideEntryDto(
    Guid Id,
    string SizeLabel,
    string? AlternativeLabel,
    decimal? Chest,
    decimal? Waist,
    decimal? Hips,
    decimal? Inseam,
    decimal? Shoulder,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    decimal? Weight,
    IReadOnlyDictionary<string, string>? AdditionalMeasurements,
    int DisplayOrder
);
