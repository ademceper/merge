using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

public record BundleItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductImageUrl,
    decimal ProductPrice,
    int Quantity,
    int SortOrder
);
