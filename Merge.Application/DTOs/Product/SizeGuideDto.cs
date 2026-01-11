namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
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
