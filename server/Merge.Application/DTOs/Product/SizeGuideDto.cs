using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

public record SizeGuideDto(
    Guid Id,
    string Name,
    string Description,
    Guid CategoryId,
    string CategoryName,
    string? Brand,
    string Type,
    string MeasurementUnit,
    bool IsActive,
    IReadOnlyList<SizeGuideEntryDto> Entries
);
