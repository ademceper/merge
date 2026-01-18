using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Search;

public record SearchResultDto(
    IReadOnlyList<ProductDto> Products,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyList<string> AvailableBrands,
    decimal MinPrice,
    decimal MaxPrice
);
