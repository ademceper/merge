using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record ProductBundleDto(
    Guid Id,
    string Name,
    string Description,
    decimal BundlePrice,
    decimal? OriginalTotalPrice,
    decimal DiscountPercentage,
    string ImageUrl,
    bool IsActive,
    DateTime? StartDate,
    DateTime? EndDate,
    IReadOnlyList<BundleItemDto> Items
)
{
    // ✅ BOLUM 7.1.5: Records - Computed property
    public bool IsAvailable => IsActive && 
        (!StartDate.HasValue || DateTime.UtcNow >= StartDate.Value) &&
        (!EndDate.HasValue || DateTime.UtcNow <= EndDate.Value);
}
