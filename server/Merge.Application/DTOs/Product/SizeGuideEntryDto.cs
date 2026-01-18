using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

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
