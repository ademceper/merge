using Merge.Application.DTOs.Product;

namespace Merge.Application.DTOs.Search;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
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
