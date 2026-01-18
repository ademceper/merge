using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

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
    public bool IsAvailable => IsActive && 
        (!StartDate.HasValue || DateTime.UtcNow >= StartDate.Value) &&
        (!EndDate.HasValue || DateTime.UtcNow <= EndDate.Value);
}
