using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

public record AddProductToBundleDto(
    Guid ProductId,
    int Quantity = 1,
    int SortOrder = 0
);
