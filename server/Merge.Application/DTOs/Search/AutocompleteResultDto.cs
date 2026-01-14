namespace Merge.Application.DTOs.Search;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record AutocompleteResultDto(
    IReadOnlyList<ProductSuggestionDto> Products,
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> Brands,
    IReadOnlyList<string> PopularSearches
);
