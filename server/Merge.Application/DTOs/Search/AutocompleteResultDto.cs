namespace Merge.Application.DTOs.Search;

public record AutocompleteResultDto(
    IReadOnlyList<ProductSuggestionDto> Products,
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> Brands,
    IReadOnlyList<string> PopularSearches
);
